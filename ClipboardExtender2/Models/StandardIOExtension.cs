using System;
using System.Diagnostics;
using System.IO;
using System.Text;
//using System.Windows.Forms;
using Microsoft.Win32;

namespace ClipboardExtender
{
    class StandardIOExtension:IExtension
    {
        string file;
        string args = "";
        string currentDirectory = "";

        public void Initialize(string path)
        {
            path = Path.GetFullPath(path);
            this.currentDirectory = Path.GetDirectoryName(path);

            if (Path.GetExtension(path).TrimStart('.').ToLower() == "exe")
            {
                this.file = path;
                return;
            }

            string fileTmp = "";

            try
            {
                fileTmp = this.FindAssociatedExecutableFile(path);
            }
            catch
            {
                throw new ArgumentException("拡張子の関連付けが見付かりませんでした。");
            }

            if (fileTmp.Length == 0)
            {
                return;
            }

            int index;

            if (fileTmp.StartsWith("\"") && fileTmp.Length >= 2)
            {
                index = fileTmp.IndexOf('"', 1);
                this.file = fileTmp.Substring(1, index - 1);
            }
            else
            {
                index = fileTmp.IndexOf(' ');
                this.file = fileTmp.Substring(0, index);
            }

            args = fileTmp.Substring(index + 1);

            for (int i = 2; i <= 9; i++)
            {
                args = args.Replace("%" + i.ToString(), "");
            }

            if (args.Contains("%0") || args.Contains("%1") || args.Contains("%*"))
            {
                args = args.Replace("%0", path);
                args = args.Replace("%1", path);
                args = args.Replace("%*", path);
            }
            else
            {
                args = path + " " + args;
            }
        }

        public void Run(ExtensionArgs args)
        {
            string outString = args.Values[0];
            StringBuilder errString = new StringBuilder();
            StringBuilder returnString = new StringBuilder();

            ProcessStartInfo psi = new ProcessStartInfo();
            psi.FileName = file;
            psi.Arguments = this.args;
            if (this.currentDirectory != "")
            {
                psi.WorkingDirectory = this.currentDirectory;
            }
            psi.RedirectStandardInput = true;
            psi.RedirectStandardOutput = true;
            psi.RedirectStandardError = true;
            psi.CreateNoWindow = true;
            psi.UseShellExecute = false;

            Process p = Process.Start(psi);
            p.StandardInput.Write(outString);
            p.StandardInput.Close();

            //ReadToEnd()1回だけだとバッファが一杯になってデッドロックが発生する。
            //非同期で取得する実装だと終端の改行に関して不安定な為、同期で取得するように実装。
            while (!p.StandardOutput.EndOfStream ||
                !p.StandardError.EndOfStream ||
                !p.HasExited)
            {

                returnString.Append(p.StandardOutput.ReadToEnd());
                errString.Append(p.StandardError.ReadToEnd());
            }

            if (p.ExitCode == 0)
            {
                if (!string.IsNullOrEmpty(returnString.ToString()))
                {
                    args.IsPasteCancel = false;
                    args.Out = returnString.ToString();
                    return;
                }
            }
            else
            {/*
                MessageBox.Show(string.Format(
@"{0} {1}
エラーコード {2} で終了しました。
{3}"
, psi.FileName, psi.Arguments, p.ExitCode.ToString(), errString));

                args.IsPasteCancel = true;
                return;
            */}
            args.IsPasteCancel = true;
            return;
        }

 
        /// <summary>
        /// ファイルに関連付けられた実行ファイルのパスを取得する
        /// </summary>
        /// <param name="fileName">関連付けを調べるファイル</param>
        /// <returns>実行ファイルのパス + コマンドライン引数</returns>
        private string FindAssociatedExecutableFile(string fileName)
        {
            //拡張子を取得
            string extName = Path.GetExtension(fileName);
            //ファイルタイプを取得
            RegistryKey regKey = Registry.ClassesRoot.OpenSubKey(extName);
            if (regKey == null)
            {
                throw new Exception("見つかりませんでした。");
            }
            string fileType = (string)regKey.GetValue("");
            regKey.Close();

            //「アクションを実行するアプリケーション」を取得
            RegistryKey regKey2 = Registry.ClassesRoot.OpenSubKey(string.Format(@"{0}\shell\{1}\command", fileType, "open"));
            if (regKey2 == null)
            {
                throw new Exception("見つかりませんでした。");
            }
            string command = (string)regKey2.GetValue("");
            regKey2.Close();

            return command;
        }
    }
}
