using System;
using System.Windows.Forms;

namespace CleanFlashCommon {
    public partial class ImageCheckBox : CheckBox {
        protected override void OnCheckedChanged(EventArgs e) {
            ImageIndex = Checked ? 1 : 0;
            base.OnCheckedChanged(e);
        }
    }
}
