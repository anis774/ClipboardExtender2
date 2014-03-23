using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ClipboardExtender;
using System.IO;

using Livet;

namespace ClipboardExtender2.Models
{
    public class ExtensionTreeItem : NotificationObject
    {
        public ExtensionTreeItem(string path = null)
        {
            if (path == null) return;

            IExtension ext = null;

            switch (Path.GetExtension(path))
            {
                case ".cbx":
                    ext = new CbxExtension();
                    break;
                default:
                    ext = new StandardIOExtension();
                    break;
            }

            ext.Initialize(path);
            this.Extension = ext;
        }


        private string _Name = String.Empty;
        public string Name
        {
            get
            {
                return this._Name;
            }
            set
            {
                if (this._Name == value) return;
                this._Name = value;
                this.RaisePropertyChanged();
            }
        }



        private IExtension _Extension = null;
        public IExtension Extension
        {
            get
            {
                return this._Extension;
            }
            set
            {
                if (this._Extension == value) return;
                this._Extension = value;
                this.RaisePropertyChanged();
            }
        }



        private ObservableSynchronizedCollection<ExtensionTreeItem> _Items = new ObservableSynchronizedCollection<ExtensionTreeItem>();
        public ObservableSynchronizedCollection<ExtensionTreeItem> Items
        {
            get
            {
                return this._Items;
            }
        }
    }
}
