using System;
using System.Diagnostics;
using System.IO;
using UnityEditor;
using Debug = UnityEngine.Debug;

namespace NTY.AssetProcessor
{
    public class AssetPostprocess : AssetPostprocessor
    {
        /// <summary>
        /// 资产预处理
        /// </summary>
        void OnPreprocessAsset()
        {
            if (EditorApplication.isCompiling)
                return;
            
            var processors = AssetProcessorRegistry.FindProcessors(ProcessorTrigger.OnPreprocessAsset, assetImporter.assetPath);
            foreach (var processor in processors)
            {
                processor.Execute(assetImporter);
            }
        }

        /// <summary>
        /// 资产导入后处理
        /// </summary>
        /// <param name="importedAssets"></param>
        /// <param name="deletedAssets"></param>
        /// <param name="movedAssets"></param>
        /// <param name="movedFromAssetPaths"></param>
        static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
        {
            if (EditorApplication.isCompiling)
                return;
            
            var sw = Stopwatch.StartNew();
            
            foreach (var path in importedAssets)
            {
                var processors = AssetProcessorRegistry.FindProcessors(ProcessorTrigger.OnPostprocessAllAssets, path, PostprocessAction.Imported);
                foreach (var processor in processors)
                {
                    processor.Execute(path, PostprocessAction.Imported);
                }
            }
            
            foreach (var path in deletedAssets)
            {
                var processors = AssetProcessorRegistry.FindProcessors(ProcessorTrigger.OnPostprocessAllAssets, path, PostprocessAction.Deleted);
                foreach (var processor in processors)
                {
                    processor.Execute(path, PostprocessAction.Deleted);
                }
            }
            
            foreach (var path in movedAssets)
            {
                var processors = AssetProcessorRegistry.FindProcessors(ProcessorTrigger.OnPostprocessAllAssets, path, PostprocessAction.Moved);
                foreach (var processor in processors)
                {
                    processor.Execute(path, PostprocessAction.Moved);
                }
            }
            
            foreach (var path in movedFromAssetPaths)
            {
                var processors = AssetProcessorRegistry.FindProcessors(ProcessorTrigger.OnPostprocessAllAssets, path, PostprocessAction.MovedFrom);
                foreach (var processor in processors)
                {
                    processor.Execute(path, PostprocessAction.MovedFrom);
                }
            }
            
            sw.Stop();
            Debug.LogWarning($"【OnPostprocessAllAssets】处理完成，总耗时: {sw.Elapsed.TotalMilliseconds:F2} ms");
        }

        private void OnPreprocessTexture()
        {
            if (EditorApplication.isCompiling)
                return;
            
            var processors = AssetProcessorRegistry.FindProcessors(ProcessorTrigger.OnPreprocessTexture, assetImporter.assetPath);
            foreach (var processor in processors)
            {
                processor.Execute(assetImporter);
            }
        }

        private void OnPreprocessModel()
        {
            if (EditorApplication.isCompiling)
                return;
            
            var processors = AssetProcessorRegistry.FindProcessors(ProcessorTrigger.OnPreprocessModel, assetImporter.assetPath);
            foreach (var processor in processors)
            {
                processor.Execute(assetImporter);
            }
        }
    }
}
