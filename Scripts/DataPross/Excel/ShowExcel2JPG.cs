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

namespace CCTool.Scripts.DataPross.Excel
{
    internal class ShowExcel2JPG : Button
    {

        private Excel2JPG _excel2jpg = null;

        protected override void OnClick()
        {
            //already open?
            if (_excel2jpg != null)
                return;
            _excel2jpg = new Excel2JPG();
            _excel2jpg.Owner = FrameworkApplication.Current.MainWindow;
            _excel2jpg.Closed += (o, e) => { _excel2jpg = null; };
            _excel2jpg.Show();
            //uncomment for modal
            //_excel2jpg.ShowDialog();
        }

    }
}
