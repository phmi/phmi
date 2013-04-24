using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Windows;
using System.Windows.Input;
using Microsoft.Win32;
using Npgsql;
using PHmiClient.Controls.Input;
using PHmiClient.Utils.Configuration;
using PHmiClient.Utils.ViewInterfaces;
using PHmiModel;
using PHmiResources.Loc;
using PHmiTools.Utils.Npg;
using PHmiTools.Utils.Npg.ExIm;
using PHmiTools.ViewModels;

namespace PHmiTools.Dialogs.Project
{
    public class ExportProjectDialogViewModel : ViewModelBase<IWindow>
    {
        private readonly IExportProjectDialogService _service;

        public ExportProjectDialogViewModel() : this(null) { }

        public ExportProjectDialogViewModel(IExportProjectDialogService helper)
        {
            _service = helper ?? new ExportProjectDialogService();
            _okCommand = new DelegateCommand(OkCommandExecuted, OkCommandCanExecute);
        }

        public INpgConnectionParameters ConnectionParameters { get; set; }

        public void Export()
        {
            var dialog = new SaveFileDialog
                {
                    Filter = string.Format("{0} (*.{1})|*.{1}", Res.BackupFiles, NpgExImHelper.BackupFileExt),
                    CheckFileExists = false,
                    OverwritePrompt = true
                };
            var stringKeeper = new Settings("ProjectImEx");
            var lastFile = stringKeeper.GetString("LastFile");
            if (!string.IsNullOrEmpty(lastFile) && File.Exists(lastFile))
            {
                dialog.InitialDirectory = Path.GetDirectoryName(lastFile);
                dialog.FileName = lastFile;
            }
            if (dialog.ShowDialog(View as Window) != true)
            {
                View.DialogResult = false;
                return;
            }
            stringKeeper.SetString("LastFile", dialog.FileName);
            stringKeeper.Save();
            var fileName = dialog.FileName;
            _service.ActionHelper.Async(() => Export(fileName));
        }

        public int MaxRows = 1000;

        private void Export(string fileName)
        {
            _service.ActionHelper.Dispatch(() =>
                {
                    InProgress = true;
                    TablesStored = 0;
                    RowsStored = 0;
                });
            FileStream stream = null;
            try
            {
                stream = new FileStream(fileName, FileMode.Create);
                var binaryFormatter = _service.ExImHelper.CreateFormatter();
                var script = _service.ScriptHelper.ExtractScriptLines(PHmiModelContext.GetPHmiScriptStream());
                var tables = _service.ExImHelper.GetTables(script);
                foreach (var table in tables)
                {
                    var startParameters = new KeyValuePair<string, object>[0];
                    var primaryKeyIndexes = table.GetPrimaryKeyIndexes();
                    using (var connection = new NpgsqlConnection(ConnectionParameters.ConnectionString))
                    {
                        while (true)
                        {
                            var result = GetData(connection, table, startParameters);
                            var tableData = new TableData
                            {
                                TableName = table.Name,
                                Columns = table.Columns.Select(c => c.Name).ToArray(),
                                Data = result
                            };
                            binaryFormatter.Serialize(stream, tableData);
                            _service.ActionHelper.Dispatch(() =>
                            {
                                RowsStored += result.Length;
                            });
                            if (result.Length != MaxRows)
                            {
                                break;
                            }
                            startParameters = new KeyValuePair<string, object>[table.PrimaryKey.Length];
                            var lastData = result[MaxRows - 1];
                            for (var i = 0; i < table.PrimaryKey.Length; i++)
                            {
                                startParameters[i] = new KeyValuePair<string, object>(
                                    table.PrimaryKey[i], lastData[primaryKeyIndexes[i]]);
                            }
                        }
                    }
                    _service.ActionHelper.Dispatch(() =>
                        {
                            TablesStored++;
                        });
                }
            }
            catch (Exception exception)
            {
                _service.DialogHelper.Exception(exception, View);
            }
            finally
            {
                if (stream != null)
                    stream.Close();
            }
            _service.ActionHelper.Dispatch(() =>
                {
                    InProgress = false;
                    View.Title = Res.ExportCompleted;
                });
        }

        public object[][] GetData(
            NpgsqlConnection connection, NpgTableInfo table, KeyValuePair<string, object>[] startParameters)
        {
            Func<NpgsqlDataReader, object[]> convertFunc = reader =>
            {
                var values = new object[table.Columns.Length];
                for (var i = 0; i < table.Columns.Length; i++)
                {
                    values[i] = reader.GetValue(i);
                }
                return values;
            };
            var scriptItem = _service.ExImHelper.GetSelectScriptItem(table, MaxRows, startParameters);
            var result = _service.NpgHelper.ExecuteReader(connection, scriptItem, convertFunc);
            return result;
        }

        #region Properties

        private int _tablesStored;
        private long _rowsStored;
        private bool _inProgress;

        public bool InProgress
        {
            get { return _inProgress; }
            set
            {
                _inProgress = value;
                OnPropertyChanged(this, v => v.InProgress);
                _okCommand.RaiseCanExecuteChanged();
            }
        }

        public int TablesStored
        {
            get { return _tablesStored; }
            set
            {
                _tablesStored = value;
                OnPropertyChanged(this, v => v.TablesStored);
            }
        }

        public long RowsStored
        {
            get { return _rowsStored; }
            set
            {
                _rowsStored = value;
                OnPropertyChanged(this, v => v.RowsStored);
            }
        }

        #endregion

        #region OkCommand

        private readonly DelegateCommand _okCommand;

        public ICommand OkCommand { get { return _okCommand; } }

        private bool OkCommandCanExecute(object obj)
        {
            return !InProgress;
        }

        private void OkCommandExecuted(object obj)
        {
            View.DialogResult = true;
        }

        #endregion
    }
}
