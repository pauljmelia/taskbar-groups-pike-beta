namespace MadDogs.TaskbarGroups.Background.Classes
{
    using System;
    using System.IO;

    using Microsoft.WindowsAPICodePack.Shell;
    using Microsoft.WindowsAPICodePack.Taskbar;

    using WinJumpList = Microsoft.WindowsAPICodePack.Taskbar.JumpList;

    public class JumpList
    {
        private readonly WinJumpList _list;

        public JumpList(IntPtr windowHandle)
        {
            _list = WinJumpList.CreateJumpListForIndividualWindow(TaskbarManager.Instance.ApplicationId, windowHandle);
            _list.KnownCategoryToDisplay = JumpListKnownCategoryType.Recent;
        }

        public void BuildJumpList(bool allowOpenAll, string name)
        {
            var categoryPath = Path.Combine(Common.Paths.ConfigPath, name);

            var userTaskbarCategory = new JumpListCustomCategory("Taskbar Groups");

            var openEdit = new JumpListLink(Common.Paths.ExeString, "Edit Group");
            openEdit.Arguments = "editingGroupMode " + name;
            openEdit.IconReference = new IconReference(Path.Combine(categoryPath, "GroupIcon.ico"), 0);
            userTaskbarCategory.AddJumpListItems(openEdit);

            if (allowOpenAll)
            {
                var openAllShortcuts = new JumpListLink(Common.Paths.ExeString, "Open all shortcuts");
                openAllShortcuts.Arguments = name + " tskBarOpen_allGroup";
                openAllShortcuts.IconReference = new IconReference(Path.Combine(categoryPath, "GroupIcon.ico"), 0);
                userTaskbarCategory.AddJumpListItems(openAllShortcuts);
            }

            try
            {
                _list.AddCustomCategories(userTaskbarCategory);

                _list.Refresh();
            }
            catch
            {
                // ignored
            }
        }
    }
}