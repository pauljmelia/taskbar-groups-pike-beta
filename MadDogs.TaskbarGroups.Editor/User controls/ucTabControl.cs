﻿namespace MadDogs.TaskbarGroups.Editor.User_controls
{
    using System;
    using System.Windows.Forms;

    public class UcTabControl : TabControl
    {
        protected override void WndProc(ref Message m)
        {
            // Hide tabs by trapping the TCM_ADJUSTRECT message
            if (m.Msg == 0x1328 && !DesignMode)
            {
                m.Result = (IntPtr)1;
            }
            else
            {
                base.WndProc(ref m);
            }
        }
    }
}