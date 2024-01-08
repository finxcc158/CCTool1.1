using ArcGIS.Core.CIM;
using ArcGIS.Core.Data;
using ArcGIS.Core.Geometry;
using ArcGIS.Desktop.Catalog;
using ArcGIS.Desktop.Core;
using ArcGIS.Desktop.Editing;
using ArcGIS.Desktop.Extensions;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;
using ArcGIS.Desktop.Framework.Dialogs;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Layouts;
using ArcGIS.Desktop.Mapping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CCTool.Scripts.Attribute.FieldMix
{
    internal class ShowAddLayerNameToField : Button
    {

        private AddLayerNameToField _addlayernametofield = null;

        protected override void OnClick()
        {
            //already open?
            if (_addlayernametofield != null)
                return;
            _addlayernametofield = new AddLayerNameToField();
            _addlayernametofield.Owner = FrameworkApplication.Current.MainWindow;
            _addlayernametofield.Closed += (o, e) => { _addlayernametofield = null; };
            _addlayernametofield.Show();
            //uncomment for modal
            //_addlayernametofield.ShowDialog();
        }

    }
}
