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

namespace CCTool.Scripts.UI.ProWindow
{
    internal class ShowExcelToEmptyGDB : Button
    {

        private ExcelToEmptyGDB _exceltoemptygdb = null;

        protected override void OnClick()
        {
            //already open?
            if (_exceltoemptygdb != null)
                return;
            _exceltoemptygdb = new ExcelToEmptyGDB();
            _exceltoemptygdb.Owner = FrameworkApplication.Current.MainWindow;
            _exceltoemptygdb.Closed += (o, e) => { _exceltoemptygdb = null; };
            _exceltoemptygdb.Show();
            //uncomment for modal
            //_exceltoemptygdb.ShowDialog();
        }

    }
}
