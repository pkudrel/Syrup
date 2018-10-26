using System;
using System.Windows.Forms;

namespace Syrup.Common.Extension
{
    public static class ControlExtensions
    {
        public static void Invoke<T>(this T c, Action<T> action) where T : Control
        {
            if (c.InvokeRequired)
                c.Invoke(new Action<T, Action<T>>(Invoke), new object[] { c, action });
            else
                action(c);
        }


    }
}