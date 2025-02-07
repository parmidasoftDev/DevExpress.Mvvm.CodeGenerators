﻿using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text.RegularExpressions;

namespace DevExpress.Mvvm.CodeGenerators {
    class FrameworkAttributes {
        public INamedTypeSymbol ViewModelAttributeSymbol { get; }
        public INamedTypeSymbol PropertyAttributeSymbol { get; }
        public INamedTypeSymbol CommandAttributeSymbol { get; }
        public FrameworkAttributes(Compilation compilation, SupportedMvvm mvvm) {
            string attributeNamespace = mvvm switch {
                SupportedMvvm.None or SupportedMvvm.Dx => InitializationGenerator.DxNamespace,
                SupportedMvvm.Prism => InitializationGenerator.PrismNamespace,
                SupportedMvvm.MvvmLight => InitializationGenerator.MvvmLightNamespace,
                _ => throw new InvalidOperationException()
            };
            ViewModelAttributeSymbol = compilation.GetTypeByMetadataName($"{attributeNamespace}.GenerateViewModelAttribute")!;
            PropertyAttributeSymbol = compilation.GetTypeByMetadataName($"{attributeNamespace}.GeneratePropertyAttribute")!;
            CommandAttributeSymbol = compilation.GetTypeByMetadataName($"{attributeNamespace}.GenerateCommandAttribute")!;
        }
    }
    class DXFrameworkAttributes : FrameworkAttributes {
        public INamedTypeSymbol IDEISymbol { get; }
        public INamedTypeSymbol? ISPVMSymbol { get; }
        public INamedTypeSymbol ISSSymbol { get; }
        public INamedTypeSymbol? CancellationTokenSymbol { get; }
        public DXFrameworkAttributes(Compilation compilation, bool isWinUI) 
            : base(compilation, SupportedMvvm.Dx) {
            IDEISymbol = compilation.GetTypeByMetadataName(typeof(IDataErrorInfo).FullName)!;
            if(isWinUI) {
                ISSSymbol = compilation.GetTypeByMetadataName("DevExpress.Mvvm.ISupportUIServices")!;
                ISPVMSymbol = null;
                CancellationTokenSymbol = compilation.GetTypeByMetadataName("System.Threading.CancellationToken");
            } else {
                ISSSymbol = compilation.GetTypeByMetadataName("DevExpress.Mvvm.ISupportServices")!;
                ISPVMSymbol = compilation.GetTypeByMetadataName("DevExpress.Mvvm.ISupportParentViewModel")!;
            }
        }
    }
    class PrismFrameworkAttributes : FrameworkAttributes {
        public INamedTypeSymbol IAASymbol { get; }
        public PrismFrameworkAttributes(Compilation compilation) 
            : base(compilation, SupportedMvvm.Prism) {
            IAASymbol = compilation.GetTypeByMetadataName("Prism.IActiveAware")!;
        }
    }
    class MvmLightFrameWorkAttributes : FrameworkAttributes {
        public INamedTypeSymbol ICUSymbol { get; }
        public MvmLightFrameWorkAttributes(Compilation compilation)
            : base(compilation, SupportedMvvm.MvvmLight) {
            ICUSymbol = compilation.GetTypeByMetadataName("GalaSoft.MvvmLight.ICleanup")!;
        }
    }
    class ContextInfo {
        public GeneratorExecutionContext Context { get; }
        public Compilation Compilation { get; }

        public DXFrameworkAttributes? Dx { get; }
        public PrismFrameworkAttributes? Prism { get; }
        public MvmLightFrameWorkAttributes? MvvmLight { get; }

        public INamedTypeSymbol INPCedSymbol { get; }
        public INamedTypeSymbol INPCingSymbol { get; }
        public INamedTypeSymbol TaskSymbol { get; }
        public INamedTypeSymbol BoolSymbol { get; }
        public INamedTypeSymbol AttributeUsageSymbol { get; }

        public bool IsWinUI { get; }
        public List<SupportedMvvm> AvailableMvvm { get; }

        public ContextInfo(GeneratorExecutionContext context, Compilation compilation) {
            Context = context;
            Compilation = compilation;

            AvailableMvvm = GetAvailableMvvm(compilation);
            IsWinUI = GetIsWinUI(compilation);

            if(AvailableMvvm.Contains(SupportedMvvm.Dx) || AvailableMvvm.Contains(SupportedMvvm.None))
                Dx = new DXFrameworkAttributes(Compilation, IsWinUI);
            if(AvailableMvvm.Contains(SupportedMvvm.Prism))
                Prism = new PrismFrameworkAttributes(Compilation);
            if(AvailableMvvm.Contains(SupportedMvvm.MvvmLight))
                MvvmLight = new MvmLightFrameWorkAttributes(Compilation);

            INPCedSymbol = compilation.GetTypeByMetadataName(typeof(INotifyPropertyChanged).FullName)!;
            INPCingSymbol = compilation.GetTypeByMetadataName(typeof(INotifyPropertyChanging).FullName)!;

            TaskSymbol = compilation.GetTypeByMetadataName("System.Threading.Tasks.Task")!;
            BoolSymbol = compilation.GetTypeByMetadataName("System.Boolean")!;
            AttributeUsageSymbol = compilation.GetTypeByMetadataName("System.AttributeUsageAttribute")!;
        }

        public FrameworkAttributes GetFrameworkAttributes(SupportedMvvm mvvm) => mvvm switch {
            SupportedMvvm.None or SupportedMvvm.Dx => Dx!,
            SupportedMvvm.Prism => Prism!,
            SupportedMvvm.MvvmLight => MvvmLight!,
            _ => throw new InvalidOperationException()
        };

        public static bool GetIsWinUI(Compilation compilation) => GetIsDxMvvmAvailable(compilation) && compilation.GetTypeByMetadataName("DevExpress.Mvvm.POCO.ViewModelSource") == null;
        static bool GetIsDxMvvmAvailable(Compilation compilation) => compilation.GetTypeByMetadataName("DevExpress.Mvvm.DelegateCommand") != null;
        static bool GetIsPrismAvailable(Compilation compilation) => compilation.GetTypeByMetadataName("Prism.Commands.DelegateCommand") != null;
        static bool GetIsMvvmLightAvailable(Compilation compilation) => compilation.GetTypeByMetadataName("GalaSoft.MvvmLight.Command.RelayCommand") != null;
        public static bool GetIsMvvmLightCommandWpfAvalible(Compilation compilation) => compilation.GetTypeByMetadataName("GalaSoft.MvvmLight.CommandWpf.RelayCommand") != null;
        public static List<SupportedMvvm> GetAvailableMvvm(Compilation compilation) {
            List<SupportedMvvm> available = new();
            if(GetIsDxMvvmAvailable(compilation))
                available.Add(SupportedMvvm.Dx);
            if(GetIsPrismAvailable(compilation))
                available.Add(SupportedMvvm.Prism);
            if(GetIsMvvmLightAvailable(compilation))
                available.Add(SupportedMvvm.MvvmLight);
            if(!available.Any())
                available.Add(SupportedMvvm.None);
            return available;
        }
    }
    internal enum SupportedMvvm {
        None = 0,
        Dx = 1,
        Prism = 2,
        MvvmLight = 3,
    }
}
