﻿/* ------------------------------------------------------------------------- */
/*
 *  UserSetting.cs
 *
 *  Copyright (c) 2009 CubeSoft, Inc.
 *
 *  This program is free software: you can redistribute it and/or modify
 *  it under the terms of the GNU General Public License as published by
 *  the Free Software Foundation, either version 3 of the License, or
 *  (at your option) any later version.
 *
 *  This program is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 *  GNU General Public License for more details.
 *
 *  You should have received a copy of the GNU General Public License
 *  along with this program.  If not, see < http://www.gnu.org/licenses/ >.
 */
/* ------------------------------------------------------------------------- */
using System;
using System.IO;
using System.Diagnostics;
using Microsoft.Win32;

namespace CubePdf
{
    /* --------------------------------------------------------------------- */
    /// DocumentProperty
    /* --------------------------------------------------------------------- */
    public class DocumentProperty
    {
        /* ----------------------------------------------------------------- */
        //  プロパティの定義
        /* ----------------------------------------------------------------- */
        #region Properies

        /* ----------------------------------------------------------------- */
        /// Title
        /* ----------------------------------------------------------------- */
        public string Title
        {
            get { return _title; }
            set { _title = value; }
        }

        /* ----------------------------------------------------------------- */
        /// Author
        /* ----------------------------------------------------------------- */
        public string Author
        {
            get { return _author; }
            set { _author = value; }
        }

        /* ----------------------------------------------------------------- */
        /// Subtitle
        /* ----------------------------------------------------------------- */
        public string Subtitle
        {
            get { return _subtitle; }
            set { _subtitle = value; }
        }

        /* ----------------------------------------------------------------- */
        /// Keyword
        /* ----------------------------------------------------------------- */
        public string Keyword
        {
            get { return _keyword; }
            set { _keyword = value; }
        }

        #endregion

        /* ----------------------------------------------------------------- */
        //  変数定義
        /* ----------------------------------------------------------------- */
        #region Variables
        private string _title = "";
        private string _author = "";
        private string _subtitle = "";
        private string _keyword = "";
        #endregion
    }

    /* --------------------------------------------------------------------- */
    /// PermissionProperty
    /* --------------------------------------------------------------------- */
    public class PermissionProperty
    {
        /* ----------------------------------------------------------------- */
        //  プロパティの定義
        /* ----------------------------------------------------------------- */
        #region Properties

        /* ----------------------------------------------------------------- */
        /// Password
        /* ----------------------------------------------------------------- */
        public string Password
        {
            get { return _password; }
            set { _password = value; }
        }

        /* ----------------------------------------------------------------- */
        /// AllowPrint
        /* ----------------------------------------------------------------- */
        public bool AllowPrint
        {
            get { return _print; }
            set { _print = value; }
        }

        /* ----------------------------------------------------------------- */
        /// AllowCopy
        /* ----------------------------------------------------------------- */
        public bool AllowCopy
        {
            get { return _copy; }
            set { _copy = value; }
        }

        /* ----------------------------------------------------------------- */
        /// AllowFormInput
        /* ----------------------------------------------------------------- */
        public bool AllowFormInput
        {
            get { return _form; }
            set { _form = value; }
        }

        /* ----------------------------------------------------------------- */
        /// AllowModify
        /* ----------------------------------------------------------------- */
        public bool AllowModify
        {
            get { return _modify; }
            set { _modify = value; }
        }

        #endregion

        /* ----------------------------------------------------------------- */
        //  変数定義
        /* ----------------------------------------------------------------- */
        #region Variables
        string _password = "";
        bool _print;
        bool _copy;
        bool _form;
        bool _modify;
        #endregion
    }

    /* --------------------------------------------------------------------- */
    ///
    /// UserSetting
    ///
    /// <summary>
    /// レジストリに保存されてあるユーザ設定の取得および設定を行うクラス．
    /// </summary>
    ///
    /* --------------------------------------------------------------------- */
    public class UserSetting
    {
        /* ----------------------------------------------------------------- */
        ///
        /// Constructor
        ///
        /// <summary>
        /// CubePdf.UserSetting クラスを初期化します．引数なしの場合は，
        /// CubePDF のバージョン情報やインストールパス等，ユーザによらず
        /// 一定 (HKEY_LOCAL_MACHINE¥Software¥CubeSoft¥CubePDF 下で定義
        /// されているもの) である情報のみロードされます．
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public UserSetting()
        {
            this.MustLoad();
        }

        /* ----------------------------------------------------------------- */
        ///
        /// Constructor
        /// 
        /// <summary>
        /// CubePdf.UserSetting クラスを初期化します．true を引数に指定した
        /// 場合，引数なしでのコンストラクタの処理に加えて，ユーザ毎の
        /// 設定情報 (HKEY_CURRENT_USER¥Software¥CubeSoft¥CubePDF¥v2) も
        /// 同時にロードされます．これは，引数なしで初期化した後に Load()
        /// メソッドを実行する事と同じです．
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public UserSetting(bool load)
        {
            this.MustLoad();
            if (load) this.Load();
        }

        /* ----------------------------------------------------------------- */
        ///
        /// MustLoad
        ///
        /// <summary>
        /// CubePDF のバージョン情報やインストールパス等，ユーザによらず
        /// 一定 (HKEY_LOCAL_MACHINE¥Software¥CubeSoft¥CubePDF 下で定義
        /// されているもの) である情報のみロードします．
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        private bool MustLoad()
        {
            bool status = true;

            try
            {
                RegistryKey subkey = Registry.LocalMachine.OpenSubKey(_RegRoot, false);
                if (subkey == null) status = false;
                else
                {
                    _version = subkey.GetValue(_RegProductVersion, _RegUnknown) as string;
                    _install = subkey.GetValue(_RegInstall, _RegUnknown) as string;
                    _lib = subkey.GetValue(_RegLib, _RegUnknown) as string;
                    subkey.Close();
                }
                if (_version == null) _version = _RegUnknown;
                if (_install == null) _install = _RegUnknown;
                if (_lib == null) _lib = _RegUnknown;
            }
            catch (Exception /* err */)
            {
                status = false;
            }

            return status;
        }

        /* ----------------------------------------------------------------- */
        ///
        /// Load
        /// 
        /// <summary>
        /// ユーザ毎の設定情報をロードします．引数なしでLoad() メソッドを
        /// 実行した場合，レジストリに保存されてある情報をロードします．
        /// </summary>
        /// 
        /* ----------------------------------------------------------------- */
        public bool Load()
        {
            bool status = true;

            try
            {
                using (RegistryKey root = Registry.CurrentUser.OpenSubKey(_RegRoot + '\\' + _RegVersion, false))
                {
                    var document = new CubePdf.Settings.Document();
                    document.Read(root);
                    Load(document);
                }
            }
            catch (Exception /* err */)
            {
                status = false;
            }

            return status;
        }

        /* ----------------------------------------------------------------- */
        ///
        /// Load
        /// 
        /// <summary>
        /// ユーザ毎の設定情報をロードします．引数には，設定情報が保存されて
        /// あるXML ファイルへのパスを指定します．
        /// </summary>
        /// 
        /* ----------------------------------------------------------------- */
        public bool Load(string path)
        {
            bool status = true;

            try
            {
                throw new NotImplementedException();
            }
            catch (Exception /* err */)
            {
                status = false;
            }

            return status;
        }

        /* ----------------------------------------------------------------- */
        ///
        /// Save
        /// 
        /// <summary>
        /// CubePdf.UserSetting クラスの現在の設定をレジストリに保存します．
        /// </summary>
        /// 
        /* ----------------------------------------------------------------- */
        public bool Save()
        {
            bool status = true;

            try
            {
                var document = new CubePdf.Settings.Document();
                Save(document);

                using (var root = Registry.CurrentUser.CreateSubKey(_RegRoot + '\\' + _RegVersion))
                {
                    document.Write(root);
                }
            }
            catch (Exception /* err */)
            {
                status = false;
            }

            return status;
        }

        /* ----------------------------------------------------------------- */
        ///
        /// Save
        /// 
        /// <summary>
        /// CubePdf.UserSetting クラスの現在の設定を指定されたパスにXML
        /// 形式で保存します．
        /// </summary>
        /// 
        /* ----------------------------------------------------------------- */
        public bool Save(string path)
        {
            bool status = true;

            try
            {
                throw new NotImplementedException();
            }
            catch (Exception /* err */)
            {
                status = false;
            }

            return status;
        }

        /* ----------------------------------------------------------------- */
        /// ToString
        /* ----------------------------------------------------------------- */
        public override string ToString()
        {
            var dest = new System.Text.StringBuilder();

            dest.AppendLine("UserSetting");
            dest.AppendLine("\tGeneral");
            dest.AppendLine("\t\tVersion         = " + _version);
            dest.AppendLine("\t\tInstallPath     = " + _install);
            dest.AppendLine("\t\tLibPath         = " + _lib);
            dest.AppendLine("\t\tFileType        = " + _type.ToString());
            dest.AppendLine("\t\tPDFVersion      = " + _pdfver.ToString());
            dest.AppendLine("\t\tResolution      = " + _resolution.ToString());
            dest.AppendLine("\t\tLastAccess      = " + _output);
            dest.AppendLine("\t\tLastInputAccess = " + _input);
            dest.AppendLine("\t\tExistedFile     = " + _exist.ToString());
            dest.AppendLine("\t\tPostProcess     = " + _postproc.ToString());
            dest.AppendLine("\t\tUserProgram     = " + _program);
            dest.AppendLine("\t\tUserArguments   = " + _argument);
            dest.AppendLine("\t\tDownsampling    = " + _downsampling.ToString());
            dest.AppendLine("\t\tImageFilter     = " + _filter.ToString());
            dest.AppendLine("\t\tPageRotation    = " + _rotation.ToString());
            dest.AppendLine("\t\tEmbedFonts      = " + _embed.ToString());
            dest.AppendLine("\t\tGrayscale       = " + _grayscale.ToString());
            dest.AppendLine("\t\tWebOptimize     = " + _web.ToString());
            dest.AppendLine("\t\tSaveSetting     = " + _save.ToString());
            dest.AppendLine("\t\tUpdateCheck     = " + _update.ToString());
            dest.AppendLine("\t\tVisible         = " + _visible.ToString());
            dest.AppendLine("\t\tDeleteOnClose   = " + _delete_input.ToString());
            dest.AppendLine("\tDocument Properties");
            dest.AppendLine("\t\tTitle           = " + _doc.Title);
            dest.AppendLine("\t\tAuthor          = " + _doc.Author);
            dest.AppendLine("\t\tSubtitle        = " + _doc.Subtitle);
            dest.AppendLine("\t\tKeywords        = " + _doc.Keyword);
            dest.AppendLine("\tSecurity");
            dest.AppendLine("\t\tOwnerPassword   = " + (_permission.Password != null && _permission.Password.Length > 0));
            dest.AppendLine("\t\tUserPassword    = " + (_password != null && _password.Length > 0));
            dest.AppendLine("\t\tAllowPrint      = " + _permission.AllowPrint);
            dest.AppendLine("\t\tAllowFormInput  = " + _permission.AllowFormInput);
            dest.AppendLine("\t\tAllowCopy       = " + _permission.AllowCopy);
            dest.Append(    "\t\tAllowModify     = " + _permission.AllowModify);

            return dest.ToString();
        }

        /* ----------------------------------------------------------------- */
        //  過去のレジストリからの変換
        /* ----------------------------------------------------------------- */
        #region Upgrade from old version

        /* ----------------------------------------------------------------- */
        ///
        /// UpgradeFromV1
        ///
        /// <summary>
        /// 過去のバージョンのレジストリを読み込み，現行バージョンに対応
        /// した形に変換する．
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public bool UpgradeFromV1(string root)
        {
            bool status = true;

            try
            {
                // ユーザ設定を読み込む
                RegistryKey subkey = Registry.CurrentUser.OpenSubKey(root, false);
                if (subkey == null) return false;

                // パス関連
                string desktop = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
                string path = subkey.GetValue(_RegLastAccess, desktop) as string;
                if (path != null && path.Length > 0 && Directory.Exists(path)) _output = path;
                path = subkey.GetValue(_RegLastInputAccess, desktop) as string;
                if (path != null && path.Length > 0 && Directory.Exists(path)) _input = path;
                path = subkey.GetValue(_RegUserProgram, "") as string;
                if (path != null && path.Length > 0 && File.Exists(path)) _program = path;

                // チェックボックスのフラグ関連
                int flag = (int)subkey.GetValue(_RegPageRotation, 1);
                _rotation = (flag != 0);
                flag = (int)subkey.GetValue(_RegEmbedFont, 1);
                _embed = (flag != 0);
                flag = (int)subkey.GetValue(_RegGrayscale, 0);
                _grayscale = (flag != 0);
                flag = (int)subkey.GetValue(_RegWebOptimize, 0);
                _web = (flag != 0);
                flag = (int)subkey.GetValue(_RegSaveSetting, 0);
                _save = (flag != 0) ? Parameter.SaveSettings.Save : Parameter.SaveSettings.None;
                flag = (int)subkey.GetValue(_RegCheckUpdate, 1);
                _update = (flag != 0);
                flag = (int)subkey.GetValue(_RegAdvancedMode, 0);
                _advance = (flag != 0);
                flag = (int)subkey.GetValue(_RegSelectInput, 0);
                _selectable = (flag != 0);

                // コンボボックスの変換
                string type = (string)subkey.GetValue(_RegFileType, "");
                foreach (Parameter.FileTypes id in Enum.GetValues(typeof(Parameter.FileTypes)))
                {
                    if (Parameter.FileTypeValue(id) == type)
                    {
                        _type = id;
                        break;
                    }
                }

                string pdfver = (string)subkey.GetValue(_RegPdfVersion, "");
                foreach (Parameter.PDFVersions id in Enum.GetValues(typeof(Parameter.PDFVersions)))
                {
                    if (Parameter.PDFVersionValue(id).ToString() == pdfver)
                    {
                        _pdfver = id;
                        break;
                    }
                }

                string resolution = (string)subkey.GetValue(_RegResolution, "");
                foreach (Parameter.Resolutions id in Enum.GetValues(typeof(Parameter.Resolutions)))
                {
                    if (Parameter.ResolutionValue(id).ToString() == resolution)
                    {
                        _resolution = id;
                        break;
                    }
                }

                // ExistedFile: v1 は日本語名で直接指定されていた
                string exist = (string)subkey.GetValue(_RegExistedFile, "");
                if (exist == "上書き") _exist = Parameter.ExistedFiles.Overwrite;
                else if (exist == "先頭に結合") _exist = Parameter.ExistedFiles.MergeHead;
                else if (exist == "末尾に結合") _exist = Parameter.ExistedFiles.MergeTail;

                // PostProcess: v1 は日本語名で直接指定されていた
                string postproc = (string)subkey.GetValue(_RegPostProcess, "");
                if (postproc == "開く") _postproc = Parameter.PostProcesses.Open;
                else if (postproc == "何もしない") _postproc = Parameter.PostProcesses.None;
                else if (postproc == "ユーザープログラム") _postproc = Parameter.PostProcesses.UserProgram;

                // DownsSampling: v1 は日本語名で直接指定されていた
                string downsampling = (string)subkey.GetValue(_RegDownSampling, "");
                if (downsampling == "なし") _downsampling = Parameter.DownSamplings.None;
                else if (downsampling == "平均化") _downsampling = Parameter.DownSamplings.Average;
                else if (downsampling == "バイキュービック") _downsampling = Parameter.DownSamplings.Bicubic;
                else if (downsampling == "サブサンプル") _downsampling = Parameter.DownSamplings.Subsample;
            }
            catch (Exception /* err */)
            {
                status = false;
            }

            return status;
        }

        #endregion

        /* ----------------------------------------------------------------- */
        //  プロパティの定義
        /* ----------------------------------------------------------------- */
        #region Properties

        /* ----------------------------------------------------------------- */
        ///
        /// Extension
        ///
        /// <summary>
        /// 設定をXML ファイルとして保存する場合の拡張子を取得します．
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public string Extension
        {
            get { return _RegSettingExtension; }
        }

        /* ----------------------------------------------------------------- */
        ///
        /// Version
        ///
        /// <summary>
        /// 現在インストールされているCubePDF のバージョン情報を取得します．
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public string Version
        {
            get { return _version; }
        }

        /* ----------------------------------------------------------------- */
        ///
        /// InstallPath
        ///
        /// <summary>
        /// 現在インストールされているCubePDF のインストールフォルダへの
        /// パスを取得します．
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public string InstallPath
        {
            get { return _install; }
        }

        /* ----------------------------------------------------------------- */
        ///
        /// LibPath
        /// 
        /// <summary>
        /// 現在インストールされているCubePDF が利用しているライブラリへの
        /// パスを取得します．LibPath プロパティで取得されるパスには，
        /// 主に，Ghostscript が使用する各種ライブラリが保存されています．
        /// </summary>
        /// 
        /* ----------------------------------------------------------------- */
        public string LibPath
        {
            get { return _lib; }
        }

        /* ----------------------------------------------------------------- */
        /// InputPath
        /* ----------------------------------------------------------------- */
        public string InputPath
        {
            get { return _input; }
            set { _input = value; }
        }

        /* ----------------------------------------------------------------- */
        ///
        /// OutputPath
        /// 
        /// <summary>
        /// 出力パスを取得，または設定します．
        /// </summary>
        /// 
        /* ----------------------------------------------------------------- */
        public string OutputPath
        {
            get { return _output; }
            set { _output = value; }
        }

        /* ----------------------------------------------------------------- */
        ///
        /// UserProgram
        /// 
        /// <summary>
        /// CubePDF 実行終了後に実行されるプログラムへのパスを取得，または
        /// 設定します．尚，PostProcess プロパティに指定されている値が
        /// CubePdf.Parameter.PostProcess.UserProgram 以外の場合，この設定は
        /// 無視されます．
        /// </summary>
        /// 
        /* ----------------------------------------------------------------- */
        public string UserProgram
        {
            get { return _program; }
            set { _program = value; }
        }

        /* ----------------------------------------------------------------- */
        ///
        /// UserArguments
        /// 
        /// <summary>
        /// CubePDF 実行終了後に実行されるプログラムへ指定する引数を取得，
        /// または設定します．尚，この設定の有無に関わらず，第 1 引数には
        /// CubePDF によって生成（変換）されたファイルへのパスが指定されます．
        /// そのため，UserArguments で指定された引数は第2 引数以降に指定され
        /// ます．
        /// </summary>
        /// 
        /* ----------------------------------------------------------------- */
        public string UserArguments
        {
            get { return _argument; }
            set { _argument = value; }
        }

        /* ----------------------------------------------------------------- */
        ///
        /// FileType
        ///
        /// <summary>
        /// 変換するファイルの種類を取得、または設定します．
        /// 設定可能な値は以下の通りです:
        /// PDF, PS, EPS, SVG, PNG, JPEG, BMP, TIFF
        /// これらの値は，例えば，CubePdf.Parameter.FileTypes.PDF のように
        /// 設定します．
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public Parameter.FileTypes FileType
        {
            get { return _type; }
            set { _type = value; }
        }

        /* ----------------------------------------------------------------- */
        ///
        /// PDFVersion
        ///
        /// <summary>
        /// PDF 形式で変換する場合の PDF バージョンを取得，または設定します．
        /// 設定可能な値は以下の通りです:
        /// Ver1_7, Ver1_6, Ver1_5, Ver1_4, Ver1_3, Ver1_2
        /// これらの値は，例えば，CubePdf.Parameter.PDFVersions.Ver1_7
        /// のように設定します．
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public Parameter.PDFVersions PDFVersion
        {
            get { return _pdfver; }
            set { _pdfver = value; }
        }

        /* ----------------------------------------------------------------- */
        ///
        /// Resolution
        ///
        /// <summary>
        /// ビットマップ形式で保存する場合の解像度を取得，または設定します．
        /// 設定可能な値は以下の通りです:
        /// Resolution72, Resolution150, Resolution300, Resolution450,
        /// Resolution600
        /// これらの値は，例えば，CubePdf.Parameter.Resolutions.Resolution72
        /// のように設定しま．
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public Parameter.Resolutions Resolution
        {
            get { return _resolution; }
            set { _resolution = value; }
        }

        /* ----------------------------------------------------------------- */
        ///
        /// ExistedFile
        ///
        /// <summary>
        /// OutputPath プロパティで指定されたファイルが既に存在する場合の
        /// 処理を取得，または設定します．設定可能な値は以下の通りです:
        /// Overwrite : 上書き
        /// MergeHead : 既に存在するファイルの先頭に結合
        /// MergeTail : 既に存在するファイルの末尾に結合
        /// Rename    : Sample (2).pdf のようにリネーム
        /// これらの値は，例えば，CubePdf.Parameter.ExistedFiles.Overwrite
        /// のように設定します．
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public Parameter.ExistedFiles ExistedFile
        {
            get { return _exist; }
            set { _exist = value; }
        }

        /* ----------------------------------------------------------------- */
        ///
        /// PostProcess
        ///
        /// <summary>
        /// CubePDF 実行終了後の処理を取得，または設定します．
        /// 設定可能な値は以下の通りです:
        /// Open : 関連付けられているアプリケーションでファイルを開く
        /// None : 何もしない
        /// UserProgram : UserProgram で設定されたプログラムを実行する
        /// これらの値は，例えば，CubePdf.Parameter.PostProcesses.Open
        /// のように設定します．
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public Parameter.PostProcesses PostProcess
        {
            get { return _postproc; }
            set { _postproc = value; }
        }

        /* ----------------------------------------------------------------- */
        ///
        /// DownSampling
        ///
        /// <summary>
        /// 画像のダウンサンプリング方法を取得，または設定します．
        /// 設定可能な値は以下の通りです:
        /// None, Average, Bicubic, Subsample
        /// これらの値は，例えば，CubePdf.Parameter.DownSamplings.None
        /// のように設定します．
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public Parameter.DownSamplings DownSampling
        {
            get { return _downsampling; }
            set { _downsampling = value; }
        }

        /* ----------------------------------------------------------------- */
        ///
        /// ImageFilter
        /// 
        /// <summary>
        /// 画像の圧縮方法を取得，または設定します．
        /// 設定可能な値は以下の通りです:
        /// FlateEncode, DCTEncode, CCITTFaxEncode
        /// これらの値は，例えば，CubePdf.Parameter.ImageFilter.FlateEncode
        /// のように設定します．
        /// </summary>
        /// 
        /* ----------------------------------------------------------------- */
        public Parameter.ImageFilters ImageFilter
        {
            get { return _filter; }
            set { _filter = value; }
        }

        /* ----------------------------------------------------------------- */
        ///
        /// SaveSetting
        ///
        /// <summary>
        /// 現在の設定をレジストリに保存するかどうかを取得、または設定
        /// します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public Parameter.SaveSettings SaveSetting
        {
            get { return _save; }
            set { _save = value; }
        }
        
        /* ----------------------------------------------------------------- */
        ///
        /// PageRotation
        ///
        /// <summary>
        /// ページの自動回転を行うかどうかを取得，または設定します．
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public bool PageRotation
        {
            get { return _rotation; }
            set { _rotation = value; }
        }

        /* ----------------------------------------------------------------- */
        ///
        /// EmbedFont
        ///
        /// <summary>
        /// フォントを埋め込むかどうかを取得，または設定します．
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public bool EmbedFont
        {
            get { return _embed; }
            set { _embed = value; }
        }

        /* ----------------------------------------------------------------- */
        ///
        /// Grayscale
        /// 
        /// <summary>
        /// 画像をグレースケースにするかどうかを取得，または設定します．
        /// </summary>
        /// 
        /* ----------------------------------------------------------------- */
        public bool Grayscale
        {
            get { return _grayscale; }
            set { _grayscale = value; }
        }

        /* ----------------------------------------------------------------- */
        ///
        /// WebOptimize
        ///
        /// <summary>
        /// PDF ファイルの場合に，Web 表示用に最適化するかどうかを取得，
        /// または設定します．
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public bool WebOptimize
        {
            get { return _web; }
            set { _web = value; }
        }

        /* ----------------------------------------------------------------- */
        ///
        /// CheckUpdate
        ///
        /// <summary>
        /// PC 起動時にアップデートチェックプログラムを実行するかどうかを
        /// 取得，または設定します．
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public bool CheckUpdate
        {
            get { return _update; }
            set { _update = value; }
        }

        /* ----------------------------------------------------------------- */
        ///
        /// CheckUpdate
        ///
        /// <summary>
        /// CubePDF メイン画面を表示するかどうかを取得、または設定します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public bool Visible
        {
            get { return _visible; }
            set { _visible = value; }
        }

        /* ----------------------------------------------------------------- */
        ///
        /// AdvancedMode
        ///
        /// <summary>
        /// CubePDF をアドバンスモードで起動するかどうかを取得，または設定
        /// します．CubePDF をアドバンスモードで起動するとユーザプログラム
        /// を使用できるようになります．
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public bool AdvancedMode
        {
            get { return _advance; }
            set { _advance = value; }
        }

        /* ----------------------------------------------------------------- */
        ///
        /// SelectInputFile
        ///
        /// <summary>
        /// 入力ファイルを指定するダイアログを表示するかどうかを取得，
        /// または設定します．
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public bool SelectInputFile
        {
            get { return _selectable; }
            set { _selectable = value; }
        }

        /* ----------------------------------------------------------------- */
        ///
        /// DeleteOnClose
        ///
        /// <summary>
        /// 変換終了時に入力ファイルを削除するかどうかを取得、または設定
        /// します。このプロパティは、レジストリに保存されません。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        public bool DeleteOnClose
        {
            get { return _delete_input; }
            set { _delete_input = value; }
        }

        /* ----------------------------------------------------------------- */
        /// Document
        /* ----------------------------------------------------------------- */
        public DocumentProperty Document
        {
            get { return _doc; }
            set { _doc = value; }
        }

        /* ----------------------------------------------------------------- */
        /// Permission
        /* ----------------------------------------------------------------- */
        public PermissionProperty Permission
        {
            get { return _permission; }
            set { _permission = value; }
        }

        /* ----------------------------------------------------------------- */
        /// Password
        /* ----------------------------------------------------------------- */
        public string Password
        {
            get { return _password; }
            set { _password = value; }
        }

        #endregion

        /* ----------------------------------------------------------------- */
        //  内部処理のためのメソッド群
        /* ----------------------------------------------------------------- */
        #region Private methods

        /* ----------------------------------------------------------------- */
        ///
        /// Load
        ///
        /// <summary>
        /// CubePdf.Settings.Document オブジェクトから必要な情報をロード
        /// します。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        private void Load(CubePdf.Settings.Document document)
        {
            LoadPaths(document);
            LoadFlags(document);
            LoadIndices(document);
        }

        /* ----------------------------------------------------------------- */
        ///
        /// LoadPaths
        ///
        /// <summary>
        /// CubePdf.Settings.Document オブジェクトからパス関連の情報を
        /// 抽出して、対応する変数にロードします。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        private void LoadPaths(CubePdf.Settings.Document document)
        {
            var desktop = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);

            var output = document.Root.Find(_RegLastAccess);
            if (output != null) _output = output.GetValue(_output);
            if (string.IsNullOrEmpty(_output)) _output = desktop;

            var input = document.Root.Find(_RegLastInputAccess);
            if (input != null) _input = input.GetValue(_input);
            if (string.IsNullOrEmpty(_input)) _input = desktop;

            var program = document.Root.Find(_RegUserProgram);
            if (program != null) _program = program.GetValue(_program);

            var argument = document.Root.Find(_RegUserArguments);
            if (argument != null) _argument = argument.GetValue(_argument);
        }

        /* ----------------------------------------------------------------- */
        ///
        /// LoadFlags
        ///
        /// <summary>
        /// CubePdf.Settings.Document オブジェクトから CubePDF メイン画面で
        /// 表示されているチェックボックスのフラグ関連の情報を抽出して、
        /// 対応する変数にロードします。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        private void LoadFlags(CubePdf.Settings.Document document)
        {
            var rotation = document.Root.Find(_RegPageRotation);
            if (rotation != null) _rotation = ((int)rotation.Value != 0);

            var embed = document.Root.Find(_RegEmbedFont);
            if (embed != null) _embed = ((int)embed.Value != 0);

            var grayscale = document.Root.Find(_RegGrayscale);
            if (grayscale != null) _grayscale = ((int)grayscale.Value != 0);

            var web = document.Root.Find(_RegWebOptimize);
            if (web != null) _web = ((int)web.Value != 0);

            var update = document.Root.Find(_RegCheckUpdate);
            if (update != null) _update = ((int)update.Value != 0);

            var advance = document.Root.Find(_RegAdvancedMode);
            if (advance != null) _advance = ((int)advance.Value != 0);

            var visible = document.Root.Find(_RegVisible);
            if (visible != null) _visible = ((int)visible.Value != 0);

            var selectable = document.Root.Find(_RegSelectInput);
            if (selectable != null) _selectable = ((int)selectable.Value != 0);
        }

        /* ----------------------------------------------------------------- */
        ///
        /// LoadIndices
        ///
        /// <summary>
        /// CubePdf.Settings.Document オブジェクトから CubePDF メイン画面で
        /// 表示されているコンボボックスのインデックス関連の情報を抽出して、
        /// 対応する変数にロードします。
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        private void LoadIndices(CubePdf.Settings.Document document)
        {
            var filetype = document.Root.Find(_RegFileType);
            if (filetype != null)
            {
                foreach (int item in Enum.GetValues(typeof(Parameter.FileTypes)))
                {
                    if (item == (int)filetype.Value)
                    {
                        _type = (Parameter.FileTypes)filetype.Value;
                        break;
                    }
                }
            }

            var pdfversion = document.Root.Find(_RegPdfVersion);
            if (pdfversion != null)
            {
                foreach (int item in Enum.GetValues(typeof(Parameter.PDFVersions)))
                {
                    if (item == (int)pdfversion.Value)
                    {
                        _pdfver = (Parameter.PDFVersions)pdfversion.Value;
                        break;
                    }
                }
            }

            var resolution = document.Root.Find(_RegResolution);
            if (resolution != null)
            {
                foreach (int item in Enum.GetValues(typeof(Parameter.Resolutions)))
                {
                    if (item == (int)resolution.Value)
                    {
                        _resolution = (Parameter.Resolutions)resolution.Value;
                        break;
                    }
                }
            }

            var exist = document.Root.Find(_RegExistedFile);
            if (exist != null)
            {
                foreach (int item in Enum.GetValues(typeof(Parameter.ExistedFiles)))
                {
                    if (item == (int)exist.Value)
                    {
                        _exist = (Parameter.ExistedFiles)exist.Value;
                        break;
                    }
                }
            }

            var postproc = document.Root.Find(_RegPostProcess);
            if (postproc != null)
            {
                foreach (int item in Enum.GetValues(typeof(Parameter.PostProcesses)))
                {
                    if (item == (int)postproc.Value)
                    {
                        _postproc = (Parameter.PostProcesses)postproc.Value;
                        break;
                    }
                }
            }

            var downsampling = document.Root.Find(_RegDownSampling);
            if (downsampling != null)
            {
                foreach (int item in Enum.GetValues(typeof(Parameter.DownSamplings)))
                {
                    if (item == (int)downsampling.Value)
                    {
                        _downsampling = (Parameter.DownSamplings)downsampling.Value;
                        break;
                    }
                }
            }

            var filter = document.Root.Find(_RegImageFilter);
            if (filter != null)
            {
                foreach (int item in Enum.GetValues(typeof(Parameter.ImageFilters)))
                {
                    if (item == (int)filter.Value)
                    {
                        _filter = (Parameter.ImageFilters)filter.Value;
                        break;
                    }
                }
            }

            var save = document.Root.Find(_RegSaveSetting);
            if (save != null)
            {
                foreach (int item in Enum.GetValues(typeof(Parameter.SaveSettings)))
                {
                    if (item == (int)save.Value)
                    {
                        _save = (Parameter.SaveSettings)save.Value;
                        break;
                    }
                }
            }
        }

        /* ----------------------------------------------------------------- */
        ///
        /// Save
        ///
        /// <summary>
        /// 設定情報を ParameterList クラスに保存する．尚，アップデートを
        /// チェックする項目のみ，チェックの有無にしたがって Startup に
        /// 関係するレジストリの内容を変更しなければならないので，その処理
        /// もこのメソッドで行う．
        /// </summary>
        ///
        /* ----------------------------------------------------------------- */
        private void Save(CubePdf.Settings.Document document)
        {
            // パス関連
            document.Root.Add(new CubePdf.Settings.Node(_RegLastAccess, _output));
            document.Root.Add(new CubePdf.Settings.Node(_RegLastInputAccess, _input));
            document.Root.Add(new CubePdf.Settings.Node(_RegUserProgram, _program));
            document.Root.Add(new CubePdf.Settings.Node(_RegUserArguments, _argument));

            // チェックボックスのフラグ関連
            int flag = _rotation ? 1 : 0;
            document.Root.Add(new CubePdf.Settings.Node(_RegPageRotation, flag));
            flag = _embed ? 1 : 0;
            document.Root.Add(new CubePdf.Settings.Node(_RegEmbedFont, flag));
            flag = _grayscale ? 1 : 0;
            document.Root.Add(new CubePdf.Settings.Node(_RegGrayscale, flag));
            flag = _web ? 1 : 0;
            document.Root.Add(new CubePdf.Settings.Node(_RegWebOptimize, flag));
            flag = _update ? 1 : 0;
            document.Root.Add(new CubePdf.Settings.Node(_RegCheckUpdate, flag));
            flag = _advance ? 1 : 0;
            document.Root.Add(new CubePdf.Settings.Node(_RegAdvancedMode, flag));
            flag = _visible ? 1 : 0;
            document.Root.Add(new CubePdf.Settings.Node(_RegVisible, flag));
            flag = _selectable ? 1 : 0;
            document.Root.Add(new CubePdf.Settings.Node(_RegSelectInput, flag));

            // コンボボックスのインデックス関連
            document.Root.Add(new CubePdf.Settings.Node(_RegFileType, (int)_type));
            document.Root.Add(new CubePdf.Settings.Node(_RegPdfVersion, (int)_pdfver));
            document.Root.Add(new CubePdf.Settings.Node(_RegResolution, (int)_resolution));
            document.Root.Add(new CubePdf.Settings.Node(_RegExistedFile, (int)_exist));
            document.Root.Add(new CubePdf.Settings.Node(_RegPostProcess, (int)_postproc));
            document.Root.Add(new CubePdf.Settings.Node(_RegDownSampling, (int)_downsampling));
            document.Root.Add(new CubePdf.Settings.Node(_RegImageFilter, (int)_filter));
            document.Root.Add(new CubePdf.Settings.Node(_RegSaveSetting, (int)_save));

            // アップデートプログラムの登録および削除
            using (var startup = Registry.CurrentUser.CreateSubKey(@"Software\Microsoft\Windows\CurrentVersion\Run"))
            {
                if (_update)
                {
                    string value = startup.GetValue(_RegUpdateProgram) as string;
                    if (startup.GetValue(_RegUpdateProgram) == null && _install.Length > 0)
                    {
                        startup.SetValue(_RegUpdateProgram, '"' + _install + '\\' + _RegUpdateProgram + ".exe\"");
                    }
                }
                else startup.DeleteValue(_RegUpdateProgram, false);
            }
        }

        #endregion

        #region Variables
        private string _install = _RegUnknown;
        private string _lib = _RegUnknown;
        private string _version = _RegUnknown;
        private string _input = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
        private string _output = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
        private string _program = "";
        private string _argument = "%%FILE%%";
        private string _password = "";
        private Parameter.FileTypes _type = Parameter.FileTypes.PDF;
        private Parameter.PDFVersions _pdfver = Parameter.PDFVersions.Ver1_7;
        private Parameter.Resolutions _resolution = Parameter.Resolutions.Resolution300;
        private Parameter.ExistedFiles _exist = Parameter.ExistedFiles.Overwrite;
        private Parameter.PostProcesses _postproc = Parameter.PostProcesses.Open;
        private Parameter.DownSamplings _downsampling = Parameter.DownSamplings.None;
        private Parameter.ImageFilters _filter = Parameter.ImageFilters.FlateEncode;
        private Parameter.SaveSettings _save = Parameter.SaveSettings.None;
        private bool _rotation = true;
        private bool _embed = true;
        private bool _grayscale = false;
        private bool _web = false;
        private bool _update = true;
        private bool _visible = true;
        private bool _advance = false;
        private bool _selectable = false;
        private bool _delete_input = false;
        private DocumentProperty _doc = new DocumentProperty();
        private PermissionProperty _permission = new PermissionProperty();
        #endregion

        #region Constant variables
        private static readonly string _RegRoot = @"Software\CubeSoft\CubePDF";
        private static readonly string _RegVersion = "v2";
        private static readonly string _RegInstall = "InstallPath";
        private static readonly string _RegLib = "LibPath";
        private static readonly string _RegProductVersion = "Version";
        private static readonly string _RegAdvancedMode = "AdvancedMode";
        private static readonly string _RegCheckUpdate = "CheckUpdate";
        private static readonly string _RegVisible = "Visible";
        private static readonly string _RegDownSampling = "DownSampling";
        private static readonly string _RegImageFilter = "ImageFilter";
        private static readonly string _RegEmbedFont = "EmbedFont";
        private static readonly string _RegExistedFile = "ExistedFile";
        private static readonly string _RegFileType = "FileType";
        private static readonly string _RegGrayscale = "Grayscale";
        private static readonly string _RegLastAccess = "LastAccess";
        private static readonly string _RegLastInputAccess = "LastInputAccess";
        private static readonly string _RegPageRotation = "PageRotation";
        private static readonly string _RegPdfVersion = "PDFVersion";
        private static readonly string _RegPostProcess = "PostProcess";
        private static readonly string _RegResolution = "Resolution";
        private static readonly string _RegSelectInput = "SelectInputFile";
        private static readonly string _RegUserProgram = "UserProgram";
        private static readonly string _RegUserArguments = "UserArguments";
        private static readonly string _RegWebOptimize = "WebOptimize";
        private static readonly string _RegSaveSetting = "SaveSetting";
        private static readonly string _RegUnknown = "Unknown";
        private static readonly string _RegUpdateProgram = "cubepdf-checker";
        private static readonly string _RegSettingExtension = ".cubepdfconf";
        #endregion
    }
}
