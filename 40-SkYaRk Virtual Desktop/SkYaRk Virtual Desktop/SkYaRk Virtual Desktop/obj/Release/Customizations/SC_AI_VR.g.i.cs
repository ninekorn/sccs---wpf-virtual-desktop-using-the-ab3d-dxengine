﻿#pragma checksum "..\..\..\Customizations\SC_AI_VR.xaml" "{ff1816ec-aa5e-4d10-87f7-6f4963833460}" "3DE2D1F6AE479D646C17917D7FFB99D82920C1A2"
//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

using Ab3d.DirectX.Controls;
using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.Windows.Media.TextFormatting;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Shell;


namespace SkYaRk_Virtual_Desktop.Customizations {
    
    
    /// <summary>
    /// SC_AI_VR
    /// </summary>
    public partial class SC_AI_VR : System.Windows.Controls.Page, System.Windows.Markup.IComponentConnector {
        
        
        #line 12 "..\..\..\Customizations\SC_AI_VR.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Grid RootGrid;
        
        #line default
        #line hidden
        
        
        #line 13 "..\..\..\Customizations\SC_AI_VR.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Border ViewportBorder;
        
        #line default
        #line hidden
        
        
        #line 14 "..\..\..\Customizations\SC_AI_VR.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal Ab3d.DirectX.Controls.DXViewportView MainDXViewportView;
        
        #line default
        #line hidden
        
        
        #line 16 "..\..\..\Customizations\SC_AI_VR.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Viewport3D MainViewport;
        
        #line default
        #line hidden
        
        
        #line 18 "..\..\..\Customizations\SC_AI_VR.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Media.Media3D.ModelVisual3D model;
        
        #line default
        #line hidden
        
        
        #line 20 "..\..\..\Customizations\SC_AI_VR.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Media.Media3D.Model3DGroup group;
        
        #line default
        #line hidden
        
        private bool _contentLoaded;
        
        /// <summary>
        /// InitializeComponent
        /// </summary>
        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [System.CodeDom.Compiler.GeneratedCodeAttribute("PresentationBuildTasks", "4.0.0.0")]
        public void InitializeComponent() {
            if (_contentLoaded) {
                return;
            }
            _contentLoaded = true;
            System.Uri resourceLocater = new System.Uri("/SkYaRk Virtual Desktop;component/customizations/sc_ai_vr.xaml", System.UriKind.Relative);
            
            #line 1 "..\..\..\Customizations\SC_AI_VR.xaml"
            System.Windows.Application.LoadComponent(this, resourceLocater);
            
            #line default
            #line hidden
        }
        
        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [System.CodeDom.Compiler.GeneratedCodeAttribute("PresentationBuildTasks", "4.0.0.0")]
        [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Never)]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes")]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1800:DoNotCastUnnecessarily")]
        void System.Windows.Markup.IComponentConnector.Connect(int connectionId, object target) {
            switch (connectionId)
            {
            case 1:
            this.RootGrid = ((System.Windows.Controls.Grid)(target));
            return;
            case 2:
            this.ViewportBorder = ((System.Windows.Controls.Border)(target));
            return;
            case 3:
            this.MainDXViewportView = ((Ab3d.DirectX.Controls.DXViewportView)(target));
            return;
            case 4:
            this.MainViewport = ((System.Windows.Controls.Viewport3D)(target));
            return;
            case 5:
            this.model = ((System.Windows.Media.Media3D.ModelVisual3D)(target));
            return;
            case 6:
            this.group = ((System.Windows.Media.Media3D.Model3DGroup)(target));
            return;
            }
            this._contentLoaded = true;
        }
    }
}
