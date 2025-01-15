// Copyright © Mad-Dogs. All rights reserved.

namespace MadDogs.TaskbarGroups.Common.Model
{
    using System.Collections.Generic;
    using System.Drawing;

    internal class CategoryBase
    {
        public bool AllowOpenAll { get; set; }
        public string ColorString { get; set; } = ColorTranslator.ToHtml(Color.FromArgb(31, 31, 31));
        public string HoverColor { get; set; }
        public int IconSize { get; set; } = 30;
        public string Name { get; set; } = "";
        public double Opacity { get; set; } = 10;
        public int Separation { get; set; } = 8;
        public List<ProgramShortcut> ShortcutList { get; set; }
        public int Width { get; set; }
    }
}