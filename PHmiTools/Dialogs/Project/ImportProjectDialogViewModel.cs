using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using Microsoft.Win32;
using Npgsql;
using PHmiClient.Controls.Input;
using PHmiClient.Utils;
using PHmiClient.Utils.Configuration;
using PHmiModel;
using PHmiResources.Loc;
using PHmiTools.Utils.Npg;
using PHmiTools.Utils.Npg.ExIm;
using Settings = PHmiClient.Utils.Configuration.Settings;

namespace PHmiTools.Dialogs.Project
{
    public class ImportProjectDialogViewModel : ProjectDialogViewModel, IDataErrorInfo
    {
        public ImportProjectDialogViewModel() : this(null) { }

        public ImportProjectDialogViewModel(IImportProjectDialogService service)
            : base(service)
        {
            _service = service ?? new ImportProjectDialogService();
            _chooseFileNameCommand = new DelegateCommand(ChooseFileNameCommandExecuted);
        }

        private readonly IImportProjectDialogService _service;

        #region FileName

        private string _fileName;

        [LocDisplayName("File", ResourceType = typeof(Res))]
        [Required(ErrorMessageResourceName = "RequiredErrorMessage", ErrorMessageResourceType = typeof(Res))]
        public string FileName
        {
            get { return _fileName; }
            set
            {
                _fileName = value;
                OnPropertyChanged(this, v => v.FileName);
                RaiseOkCommandCanExecuteChanged();
            }
        }

        #endregion

        #region ChooseFileNameCommand

        private readonly ICommand _chooseFileNameCommand;

        public ICommand ChooseFileNameCommand { get { return _chooseFileNameCommand; } }

        private void ChooseFileNameCommandExecuted(object obj)
        {
            ShowChooseDialog(false);
        }

        public void ShowChooseDialog(bool closeOnEscape)
        {
            var dialog = new OpenFileDialog
                {
                    Filter = string.Format("{0} (*.{1})|*.{1}", Res.BackupFiles, NpgExImHelper.BackupFileExt),
                    CheckFileExists = true,
                    Multiselect = false
                };
            var stringKeeper = new Settings("ProjectImEx");
            var lastFile = stringKeeper.GetString("LastFile");
            if (!string.IsNullOrEmpty(lastFile) && File.Exists(lastFile))
            {
                dialog.InitialDirectory = Path.GetDirectoryName(lastFile);
                dialog.FileName = lastFile;
            }
            if (dialog.ShowDialog(View as Window) == true)
            {
                stringKeeper.SetString("LastFile", dialog.FileName);
                stringKeeper.Save();
                FileName = dialog.FileName;
                if (string.IsNullOrEmpty(ConnectionParameters.Database))
                {
                    ConnectionParameters.Database = Path.GetFileNameWithoutExtension(FileName);
                }
            }
            else if (closeOnEscape)
            {
                View.DialogResult = false;
            }
        }

        #endregion

        #region OkCommand

        protected override bool OkCommandCanExecute(object obj)
        {
            return !string.IsNullOrEmpty(FileName) && base.OkCommandCanExecute(obj);
        }

        protected override void OkCommandExecuted(object obj)
        {
            InProgress = true;
            ProgressIsIndeterminate = true;
            var script = _service.ScriptHelper.ExtractScriptLines(PHmiModelContext.GetPHmiScriptStream());
            Progress = 0;
            ProgressMax = script.Length;
            _service.ActionHelper.Async(() =>
            {
                FileStream stream = null;
                try
                {
                    _service.NpgHelper.CreateDatabase(ConnectionParameters);
                    _service.ActionHelper.Dispatch(() => { ProgressIsIndeterminate = false; });
                    _service.NpgHelper.ExecuteScript(
                        ConnectionParameters.ConnectionString,
                        script.Select(r => new NpgQuery(r)).ToArray(),
                        true,
                        row => _service.ActionHelper.Dispatch(() => { Progress = row; }));

                    _service.ActionHelper.Dispatch(() =>
                        {
                            ProgressIsIndeterminate = true;
                        });
                    stream = File.OpenRead(FileName);
                    var binaryFormatter = _service.ExImHelper.CreateFormatter();
                    var prevTable = string.Empty;
                    var createScript = _service.ScriptHelper.ExtractScriptLines(
                        PHmiModelContext.GetPHmiScriptStream());
                    var tables = _service.ExImHelper.GetTables(createScript);
                    var serialHelper = SerialHelper.Create(tables);
                    using (var connection = new NpgsqlConnection(ConnectionParameters.ConnectionString))
                    {
                        while (stream.Position != stream.Length)
                        {
                            var tableData = (TableData) binaryFormatter.Deserialize(stream);
                            var scriptItem = _service.ExImHelper.GetInsertScriptItem(tableData);
                            if (scriptItem == null)
                                continue;
                            var count = _service.NpgHelper.ExecuteNonQuery(connection, scriptItem);
                            var table = prevTable;
                            _service.ActionHelper.Dispatch(() =>
                                {
                                    if (table != tableData.TableName && !string.IsNullOrEmpty(table))
                                    {
                                        TablesStored++;
                                    }
                                    RowsStored += count;
                                });
                            prevTable = tableData.TableName;
                            serialHelper.Update(tableData);
                        }
                        foreach (var alterScript in serialHelper.CreateAlterScriptItem())
                        {
                            _service.NpgHelper.ExecuteNonQuery(connection, alterScript);
                        }
                    }
                    _service.DialogHelper.Message(
                        string.Format("{0} {1}{2}{3} {4}",
                                      Res.TablesStored,
                                      TablesStored.ToString("N0"),
                                      Environment.NewLine,
                                      Res.RowsStored,
                                      RowsStored.ToString("N0")),
                        Res.ImportCompleted,
                        owner: View);
                    _service.ActionHelper.Dispatch(() => base.OkCommandExecuted(obj));
                }
                catch (Exception exception)
                {
                    ExceptionDialog.Show(exception, View);
                }
                finally
                {
                    if (stream != null)
                        stream.Dispose();
                    _service.ActionHelper.Dispatch(() =>
                    {
                        InProgress = false;
                        ProgressIsIndeterminate = false;
                    });
                }
            });
        }

        #endregion

        #region Properties

        private int _tablesStored;
        private long _rowsStored;

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

        #region IDataErrorInfo

        public string this[string columnName]
        {
            get { return this.GetError(columnName); }
        }

        public string Error
        {
            get { return this.GetError(); }
        }

        #endregion
    }
}
