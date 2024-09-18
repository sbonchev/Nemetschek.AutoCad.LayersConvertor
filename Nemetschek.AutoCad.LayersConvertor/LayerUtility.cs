using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Runtime;

namespace Nemetschek.AutoCad.LayersConvertor
{
    public class LayerUtility
    {
        [CommandMethod("LCU")] // CommandFlags.Session
        public void UpdateLayer()
        {
            new LayerConvertorWindow().ShowDialog();
        }

        private Document? GetDocument(string dwgPath) {
            var docCollection = Application.DocumentManager;
            string fileName = dwgPath.Trim().ToLower(); 
            foreach (Document doc in docCollection)
            {
                if (doc.Name.Trim().ToLower() == fileName)
                    return doc;
            }

            return null;
        }

        /// <summary>
        /// Conver one layer to selected another one.
        /// </summary>
        /// <param name="dwgPath">Drawing path</param>
        /// <param name="oldLayer">From layer 1</param>
        /// <param name="newLayer">To Layer 2</param>
        internal string ProcessLayer(string dwgPath, string oldLayer, string newLayer)
        {
            var doc = GetDocument(dwgPath);
            if (doc == null)
                return "Invalid drawing path!";

            doc.GetDocumentWindow().Activate();
            var db = doc.Database;
            var editor = doc.Editor;
            try
            {
                using (doc.LockDocument())
                {
                    using (var trans = db.TransactionManager.StartOpenCloseTransaction())
                    {
                        var modelSpaceId = SymbolUtilityServices.GetBlockModelSpaceId(db);
                        var modelSpace = (BlockTableRecord)trans.GetObject(modelSpaceId, OpenMode.ForRead);
                        bool isOk = false;
                        foreach (ObjectId id in modelSpace)
                        {
                            var acEnt = trans.GetObject(id, OpenMode.ForWrite) as Entity;
                            if (acEnt == null)
                                continue;

                            var layerName = acEnt.Layer;
                            if (layerName == oldLayer)
                            {
                                acEnt.Layer = newLayer;
                                isOk = true;
                                break;
                            }
                        }
                        if (isOk)
                        {
                            trans.Commit(); 
                            db.SaveAs(dwgPath, DwgVersion.Current);
                            return $"Layer: {oldLayer} has been converted to {newLayer} successfuly!";
                        }
                        else
                            throw new Autodesk.AutoCAD.Runtime.Exception(ErrorStatus.CopyDoesNotExist, $"Cannot find layer: {oldLayer}"); 
                    }
                }
                // ------- TODO: Filter technic doesn't work (ask colleagues) --> NotApplicable Exception
                //        TypedValue[] typedValues = new TypedValue[1];
                //        typedValues.SetValue(new TypedValue((int)DxfCode.LayerName, oldLayer), 0);
                //        // --- Create multi-selection filter to process selected layer value:
                //        var filter = new SelectionFilter(typedValues);
                //        // --- Apply filter to the selected layer:
                //        PromptSelectionResult promptResult = editor.SelectAll(filter);
                //        if (promptResult.Status == PromptStatus.OK)
                //        {
                //            SelectionSet selectionSet = promptResult.Value;
                //            // --- Go through the selection set in order to change the layer:
                //            foreach (SelectedObject selectedObj in selectionSet)
                //            {
                //                if (selectedObj != null) // --- Create a new entity and assign the object to it:
                //                {
                //                    var entity = trans.GetObject(selectedObj.ObjectId, OpenMode.ForWrite) as Entity;
                //                    if (entity != null)
                //                    {
                //                        entity.Layer = newLayer;
                //                    }
                //                }
                //            }
            }
            catch (Autodesk.AutoCAD.Runtime.Exception ex)
            {
                editor.WriteMessage($"Processing Error (status - {ex.ErrorStatus}): {ex.Message} ");
                return $"Layer {oldLayer} conversion to {newLayer} failed, \n error status - {ex.ErrorStatus})!";
            }
            finally
            {
                //doc.CloseAndDiscard();
            }
        }

        /// <summary>
        /// Get all available doc's layers
        /// </summary>
        internal List<string> GetlayerList(string dwgPath)
        {
            var acDoc = GetDocument(dwgPath);
            if(acDoc == null)
                acDoc = Application.DocumentManager.Open(dwgPath);

            var acDb = acDoc.Database;
            var layerList = new List<string>();
            using (var trans = acDb.TransactionManager.StartTransaction())
            {
                try
                {
                    var modelSpaceId = SymbolUtilityServices.GetBlockModelSpaceId(acDb);
                    var modelSpace = (BlockTableRecord)trans.GetObject(modelSpaceId, OpenMode.ForRead);
                    foreach (ObjectId id in modelSpace)
                    {
                        Entity? acEnt = trans.GetObject(id, OpenMode.ForRead) as Entity;
                        if (acEnt == null) {
                             continue; 
                        }
                        var layerName = acEnt.Layer;
                        if (!layerList.Contains(layerName))
                            layerList.Add(layerName);
                    }
                    trans.Commit();
                }
                catch (Autodesk.AutoCAD.Runtime.Exception ex)
                {
                    acDoc.Editor.WriteMessage(ex.Message);
                }

                return layerList;
            }
        }

    }
}
