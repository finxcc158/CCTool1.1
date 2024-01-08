using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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

namespace CCTool.Scripts.DataPross.GDB
{
    internal class ShowMDB2GDB : Button
{

    private MDB2GDB _mdb2gdb = null;

    protected override void OnClick()
    {
        //already open?
        if (_mdb2gdb != null)
            return;
        _mdb2gdb = new MDB2GDB();
        _mdb2gdb.Owner = FrameworkApplication.Current.MainWindow;
        _mdb2gdb.Closed += (o, e) => { _mdb2gdb = null; };
        _mdb2gdb.Show();
         //uncomment for modal
         //_mdb2gdb.ShowDialog();
}

}
}
