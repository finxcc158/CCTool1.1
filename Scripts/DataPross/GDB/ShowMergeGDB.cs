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

namespace CCTool.Scripts.DataPross.GDB
{
    internal class ShowMergeGDB : Button
    {

        private MergeGDB _mergegdb = null;

        protected override void OnClick()
        {
            //already open?
            if (_mergegdb != null)
                return;
            _mergegdb = new MergeGDB();
            _mergegdb.Owner = FrameworkApplication.Current.MainWindow;
            _mergegdb.Closed += (o, e) => { _mergegdb = null; };
            _mergegdb.Show();
            //uncomment for modal
            //_mergegdb.ShowDialog();
        }

    }
}
