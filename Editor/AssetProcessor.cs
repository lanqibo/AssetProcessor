using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using UnityEditor;

namespace NTY.AssetProcessor
{
    public interface IAssetProcessor
    {
        void Execute(AssetImporter path);
        void Execute(string path);
        void Execute(string path, PostprocessAction action);
    }

    /// <summary>
    /// 扩展名处理器触发点
    /// </summary>
    [Flags]
    public enum ProcessorTrigger
    {
        None = 0,
        /// <summary>
        /// 资产预处理阶段
        /// </summary>
        OnPreprocessAsset = 1 << 1,
        /// <summary>
        /// 所有资产导入处理
        /// </summary>
        OnPostprocessAllAssets = 1 << 2,
        /// <summary>
        /// 纹理预处理阶段
        /// </summary>
        OnPreprocessTexture = 1 << 3,
        /// <summary>
        /// 模型预处理阶段（.fbx Model）
        /// </summary>
        OnPreprocessModel = 1 << 4,
        /// <summary>
        /// 动画预处理阶段（.fbx Animation）
        /// </summary>
        OnPreprocessAnimation = 1 << 5,
    }

    /// <summary>
    /// 导入所有资产的操作方式
    /// </summary>
    [Flags]
    public enum PostprocessAction
    {
        None = 0,
        Imported = 1 << 1,
        Deleted = 1 << 2,
        Moved = 1 << 3,
        MovedFrom = 1 << 4,
    }

    /// <summary>
    /// 资产处理器过滤条件
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class ProcessorConditions : Attribute
    {
        /// <summary>
        /// AssetPostprocessor触发点
        /// </summary>
        public ProcessorTrigger Triggers { get; }
        /// <summary>
        /// OnPostprocessAllsets时的触发方式
        /// </summary>
        public PostprocessAction Actions { get; }
        /// <summary>
        /// 指定文件夹
        /// </summary>
        public string[] Folders { get; }
        /// <summary>
        /// 指定扩展名
        /// </summary>
        public string[] Extensions { get; }
        /// <summary>
        /// 忽略的文件名后缀
        /// </summary>
        public string[] IgnoreSuffixes { get;}

        public ProcessorConditions(ProcessorTrigger triggers = ProcessorTrigger.None, PostprocessAction actions = PostprocessAction.None, string[] folders = null, string[] extensions = null, string[] ignoreSuffixes = null)
        {
            Triggers = triggers;
            Actions = actions;
            Folders = folders;
            Extensions = extensions;
            IgnoreSuffixes = ignoreSuffixes;
        }

        /// <summary>
        /// 触发器是否满足条件
        /// </summary>
        /// <param name="trigger"></param>
        /// <param name="action"></param>
        /// <returns></returns>
        public bool IsTrigger(ProcessorTrigger trigger, PostprocessAction action)
        {
            return Triggers.HasFlag(trigger) && Actions.HasFlag(action);
        }
        
        /// <summary>
        /// 资产扩展名和文件夹是否满足
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public bool IsMatch(string path)
        {
            return IncludedFolder(path) && IncludedExtension(path) && !IncludedIgnoreSuffix(path);
        }

        private bool IncludedFolder(string path)
        {
            if (Folders == null)
            {
                return true;
            }
            foreach (var folder in Folders)
            {
                if (path.StartsWith(folder)) return true;
            }
            return false;
        }

        private bool IncludedExtension(string path)
        {
            if (Extensions == null)
            {
                return true;
            }

            var ext = Path.GetExtension(path);
            return Extensions.Contains(ext);
        }
        
        private bool IncludedIgnoreSuffix(string path)
        {
            if (IgnoreSuffixes == null)
            {
                return false;
            }

            var fileName = Path.GetFileNameWithoutExtension(path);
            foreach (var suffix in IgnoreSuffixes)
            {
                if (fileName.EndsWith(suffix))
                {
                    return true;
                }
            }
            return false;
        }
    }

    /// <summary>
    /// 资产处理器
    /// </summary>
    public abstract class AssetProcessor : IAssetProcessor
    {
        public void Execute(AssetImporter importer)
        {
            var sw = Stopwatch.StartNew();
            OnExecute(importer);
            sw.Stop();
            UnityEngine.Debug.Log($"【{this.GetType().Name}】{importer.assetPath}: {sw.Elapsed.TotalMilliseconds:F2} ms");
        }

        protected virtual void OnExecute(AssetImporter importer)
        {
            
        }

        public void Execute(string path)
        {
            var sw = Stopwatch.StartNew();
            OnExecute(path);
            sw.Stop();
            UnityEngine.Debug.Log($"【{this.GetType().Name}】{path}: {sw.Elapsed.TotalMilliseconds:F2} ms");
        }
        
        protected virtual void OnExecute(string path)
        {
            var sw = Stopwatch.StartNew();
            OnExecute(path);
            sw.Stop();
            UnityEngine.Debug.Log($"【{this.GetType().Name}】{path}: {sw.Elapsed.TotalMilliseconds:F2} ms");
        }

        public void Execute(string path, PostprocessAction action)
        {
            var sw = Stopwatch.StartNew();
            OnExecute(path, action);
            sw.Stop();
            UnityEngine.Debug.Log($"【{this.GetType().Name}:{action}】{path}: {sw.Elapsed.TotalMilliseconds:F2} ms");
        }
        
        protected virtual void OnExecute(string path, PostprocessAction action)
        {

        }
    }
}