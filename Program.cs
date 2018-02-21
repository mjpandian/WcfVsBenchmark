﻿using System.Collections.Generic;
using System.ServiceModel;
using System.Threading.Tasks;

using BenchmarkDotNet.Analysers;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Columns;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Diagnosers;
using BenchmarkDotNet.Exporters;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Loggers;
using BenchmarkDotNet.Running;
using BenchmarkDotNet.Validators;

namespace AspNetCoreWcfBenchmark
{
    class Program
    {
        static void Main()
        {
            BenchmarkRunner.Run<WcfVsWebApiVsAspNetCoreMvc>();
        }
    }

    [Config(typeof(Config))]
    public class WcfVsWebApiVsAspNetCoreMvc
    {
        private class Config: ManualConfig
        {
            public Config()
            {
                Add(Job.RyuJitX64);
                Add(ConsoleLogger.Default);
                Add(TargetMethodColumn.Method, new ParamColumn(nameof(ItemCount)), new ParamColumn(nameof(SendItems)), new ParamColumn(nameof(ReceiveItems)), StatisticColumn.Mean, StatisticColumn.P95);
                Add(MemoryDiagnoser.Default.GetColumnProvider());
                Add(EnvironmentAnalyser.Default);
                Add(MemoryDiagnoser.Default);
                Add(JitOptimizationsValidator.DontFailOnError);
                Add(MarkdownExporter.Default);
                UnionRule = ConfigUnionRule.AlwaysUseLocal;
            }
        }

        [Params(10, 1000, 100000)]
        public int ItemCount { get; set; }

        [Params(true)]
        public bool SendItems { get; set; }

        [Params(true)]
        public bool ReceiveItems { get; set; }

        [GlobalSetup]
        public void Initialize()
        {
            _textWcfService = new WcfService(
                port: 9001,
                encoding: WSMessageEncoding.Text,
                itemCount: ItemCount,
                sendItems: SendItems,
                receiveItems: ReceiveItems
            );
            _textWcfService.Start();

            _webApiJsonNetService = new WebApiService(
                port: 9002,
                format: SerializerType.JsonNet,
                itemCount: ItemCount,
                sendItems: SendItems,
                receiveItems: ReceiveItems
            );
            _webApiJsonNetService.Start();

            _webApiMessagePackService = new WebApiService(
                port: 9003,
                format: SerializerType.MessagePack,
                itemCount: ItemCount,
                sendItems: SendItems,
                receiveItems: ReceiveItems
            );
            _webApiMessagePackService.Start();

            _webApiXmlService = new WebApiService(
                port: 9004,
                format: SerializerType.Xml,
                itemCount: ItemCount,
                sendItems: SendItems,
                receiveItems: ReceiveItems
            );
            _webApiXmlService.Start();

            _aspNetCoreJsonNetService = new AspNetCoreService(
                port: 9005,
                format:SerializerType.JsonNet,
                itemCount: ItemCount,
                sendItems: SendItems,
                receiveItems: ReceiveItems
            );
            _aspNetCoreJsonNetService.Start();

            _aspNetCoreMessagePackService = new AspNetCoreService(
                port: 9006,
                format: SerializerType.MessagePack,
                itemCount: ItemCount,
                sendItems: SendItems,
                receiveItems: ReceiveItems
            );
            _aspNetCoreMessagePackService.Start();

            _aspNetCoreXmlService = new AspNetCoreService(
                port: 9007,
                format: SerializerType.MessagePack,
                itemCount: ItemCount,
                sendItems: SendItems,
                receiveItems: ReceiveItems
            );
            _aspNetCoreXmlService.Start();
        }

        [GlobalCleanup]
        public void Cleanup()
        {
            _textWcfService?.Stop();
            _webApiJsonNetService?.Stop();
            _webApiMessagePackService?.Stop();
            _webApiXmlService?.Stop();
            _aspNetCoreJsonNetService?.Stop();
            _aspNetCoreMessagePackService?.Stop();
            _aspNetCoreXmlService?.Stop();
        }

        [Benchmark]
        public IReadOnlyCollection<Item> WcfText()
        {
            return _textWcfService.Invoke();
        }

        [Benchmark]
        public Task<IReadOnlyCollection<Item>> WebApiJsonNet()
        {
            return _webApiJsonNetService.Invoke();
        }

        [Benchmark]
        public Task<IReadOnlyCollection<Item>> WebApiMessagePack()
        {
            return _webApiMessagePackService.Invoke();
        }

        [Benchmark]
        public Task<IReadOnlyCollection<Item>> WebApiXml()
        {
            return _webApiXmlService.Invoke();
        }

        [Benchmark]
        public Task<IReadOnlyCollection<Item>> AspNetCoreJsonNet()
        {
            return _aspNetCoreJsonNetService.Invoke();
        }

        [Benchmark]
        public Task<IReadOnlyCollection<Item>> AspNetCoreMessagePack()
        {
            return _aspNetCoreMessagePackService.Invoke();
        }

        [Benchmark]
        public Task<IReadOnlyCollection<Item>> AspNetCoreXml()
        {
            return _aspNetCoreXmlService.Invoke();
        }

        private WcfService _textWcfService;
        private WebApiService _webApiJsonNetService;
        private WebApiService _webApiMessagePackService;
        private WebApiService _webApiXmlService;
        private AspNetCoreService _aspNetCoreJsonNetService;
        private AspNetCoreService _aspNetCoreMessagePackService;
        private AspNetCoreService _aspNetCoreXmlService;
    }
}
