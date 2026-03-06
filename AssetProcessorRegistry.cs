using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Compilation;

namespace NTY.AssetProcessor
{
    [InitializeOnLoad]
    public static class AssetProcessorRegistry
    {
        /// <summary>
        /// 处理器缓存
        /// </summary>
        private static readonly Dictionary<ProcessorConditions, IAssetProcessor> Processors = new();
        
        /// <summary>
        /// 排除的程序集前缀列表，避免扫描 Unity 内部程序集和系统程序集
        /// </summary>
        private static readonly List<string> ExceptionNameStartsWith = new List<string>
        {
            "UnityEditor",
            "UnityEngine",
            "Unity.",
            "System.",
            "Microsoft.", 
            "CriMw.",
            "Mono.",
            "UniTask"
        };

        static AssetProcessorRegistry()
        {
            Processors.Clear();

            // 扫描当前域所有 Assembly（或指定你的命名空间/Assembly）
            var userAssemblies = CompilationPipeline.GetAssemblies(AssembliesType.Editor)
                .Select(a => a.name).Where(n => !ExceptionNameStartsWith.Any(prefix => n.StartsWith(prefix, StringComparison.OrdinalIgnoreCase)))
                .ToHashSet(StringComparer.OrdinalIgnoreCase);
            
            var assemblies = AppDomain.CurrentDomain.GetAssemblies()
                .Where(a => !a.IsDynamic && userAssemblies.Contains(a.GetName().Name))
                .ToArray();

            foreach (var assembly in assemblies)
            {
                // 获取所有实现 IAssetProcessor 的类型
                var processorTypes = assembly.GetTypes()
                    .Where(t => typeof(IAssetProcessor).IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract);

                foreach (var type in processorTypes)
                { 
                    // 创建实例
                    var processor = (IAssetProcessor)Activator.CreateInstance(type);
                        
                    var attrs = processor.GetType().GetCustomAttributes(typeof(ProcessorConditions), false);
                    if (attrs.Length == 0)
                    {
                        UnityEngine.Debug.LogWarning($"Processor {type.FullName} does not have ProcessorConditions attribute, it will be ignored.");
                        continue;
                    }
                    var conditions = (ProcessorConditions)attrs[0];
                        
                    Processors.TryAdd(conditions, processor);
                }
            }
        }
        
        private static List<IAssetProcessor> tempList = new List<IAssetProcessor>();
        public static IAssetProcessor[] FindProcessors(ProcessorTrigger trigger, string path, PostprocessAction action = PostprocessAction.None)
        {
            tempList.Clear();
            
            foreach (var processor in Processors)
            {
                if (processor.Key.IsTrigger(trigger, action) && processor.Key.IsMatch(path))
                {
                    tempList.Add(processor.Value);
                }
            }
            return tempList.ToArray();
        }
    }
}