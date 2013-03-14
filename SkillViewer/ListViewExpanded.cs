using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;

namespace SkillViewer
{
    public partial class ListViewExpanded : ListView
    {
        private const int LVM_FIRST = 0x1000;                    // ListView messages
        private const int LVM_SETGROUPINFO = (LVM_FIRST + 147);  // ListView messages 
        // Setinfo on Group
        private const int WM_LBUTTONUP = 0x0202;                 // Windows message 
        // left button
        private delegate void CallBackSetGroupState
            (ListViewGroup lstvwgrp, ListViewGroupState state);
        private delegate void CallbackSetGroupString(ListViewGroup lstvwgrp, string value);

        [DllImport("User32.dll"), Description("Sends the specified message to a window or windows. The SendMessage function calls the window procedure for the specified window and does not return until the window procedure has processed the message. To send a message and return immediately, use the SendMessageCallback or SendNotifyMessage function. To post a message to a thread's message queue and return immediately, use the PostMessage or PostThreadMessage function.")]
        private static extern int SendMessage (IntPtr hWnd, int Msg, int wParam, LVGROUP lParam);

        private static int? GetGroupID(ListViewGroup listViewGroup)
        {
            int? rtnval = null;
            Type groupType = listViewGroup.GetType();
            if (groupType != null)
            {
                PropertyInfo pi = groupType.GetProperty("ID", BindingFlags.NonPublic | 
							    BindingFlags.Instance);
                if (pi != null)
                {
                    object tmprtnval = pi.GetValue(listViewGroup, null);
                    if (tmprtnval != null)
                    {
                        rtnval = tmprtnval as int?;
                    }
                }
            }
            return rtnval;
        }
        private static void setGrpState(ListViewGroup listViewGroup, ListViewGroupState state)
        {
            if (Environment.OSVersion.Version.Major < 6)   //Only Vista and forward 
					    // allows collapse of ListViewGroups
                return;
            if (listViewGroup == null || listViewGroup.ListView == null)
                return;
            if (listViewGroup.ListView.InvokeRequired)
                listViewGroup.ListView.Invoke(new CallBackSetGroupState(setGrpState), 
							    listViewGroup, state);
            else
            {
                int? groupId = GetGroupID(listViewGroup);
                int gIndex = listViewGroup.ListView.Groups.IndexOf(listViewGroup);
                LVGROUP group = new LVGROUP();
                group.CbSize = Marshal.SizeOf(group);
                group.State = state;
                group.Mask = ListViewGroupMask.State;
                if (groupId != null)
                {
                    group.IGroupId = groupId.Value;
                    SendMessage(listViewGroup.ListView.Handle, 
				    LVM_SETGROUPINFO, groupId.Value, group);
                    SendMessage(listViewGroup.ListView.Handle, 
				    LVM_SETGROUPINFO, groupId.Value, group);
                }
                else
                {
                    group.IGroupId = gIndex;
                    SendMessage(listViewGroup.ListView.Handle, LVM_SETGROUPINFO, gIndex, group);
                    SendMessage(listViewGroup.ListView.Handle, LVM_SETGROUPINFO, gIndex, group);
                }
                listViewGroup.ListView.Refresh();
            }
        }
        private static void setGroupFooter(ListViewGroup listViewGroup, string footer)
        {
            if (Environment.OSVersion.Version.Major < 6)   //Only Vista and forward 
						    //allows footer on ListViewGroups
                return;
            if (listViewGroup == null || listViewGroup.ListView == null)
                return;
            if (listViewGroup.ListView.InvokeRequired)
                listViewGroup.ListView.Invoke(new CallbackSetGroupString(setGroupFooter), 
							    listViewGroup, footer);
            else
            {
                int? GrpId = GetGroupID(listViewGroup);
                int gIndex = listViewGroup.ListView.Groups.IndexOf(listViewGroup);
                LVGROUP group = new LVGROUP();
                group.CbSize = Marshal.SizeOf(group);
                group.PszFooter = footer;
                group.Mask = ListViewGroupMask.Footer;
                if (GrpId != null)
                {
                    group.IGroupId = GrpId.Value;
                    SendMessage(listViewGroup.ListView.Handle, 
				    LVM_SETGROUPINFO, GrpId.Value, group);
                }
                else
                {
                    group.IGroupId = gIndex;
                    SendMessage(listViewGroup.ListView.Handle, LVM_SETGROUPINFO, gIndex, group);
                }
            }
        }
        public void SetGroupState(ListViewGroupState state)
        {
            foreach (ListViewGroup lvg in this.Groups)
                setGrpState(lvg, state);
        }
        public void SetGroupFooter(ListViewGroup listViewGroup, string footerText)
        {
                setGroupFooter(listViewGroup, footerText);
        }
        protected override void WndProc(ref Message m)
        {
            if (m.Msg == WM_LBUTTONUP)
                base.DefWndProc(ref m);
            base.WndProc(ref m);
        }
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode), 
	Description("LVGROUP StructureUsed to set and retrieve groups.")]
    public struct LVGROUP
    {
        /// <summary>
        /// Size of this structure, in bytes.
        /// </summary>
        [Description("Size of this structure, in bytes.")]
        public int CbSize;
        /// <summary>
        /// Mask that specifies which members of the structure are valid input. 
        /// One or more of the following values:LVGF_NONENo other items are valid.
        /// </summary>
        [Description("Mask that specifies which members of the structure are valid input. One or more of the following values:LVGF_NONE No other items are valid.")]
        public ListViewGroupMask Mask;
        /// <summary>
        /// Pointer to a null-terminated string that contains the header text 
        /// when item information is being set. If group information is being retrieved, 
        /// this member specifies the address of the buffer that receives the header text.
        /// </summary>
        [Description("Pointer to a null-terminated string that contains the header text when item information is being set. If group information is being retrieved, this member specifies the address of the buffer that receives the header text.")]
        [MarshalAs(UnmanagedType.LPWStr)]
        public string PszHeader;
        /// <summary>
        /// Size in TCHARs of the buffer pointed to by the pszHeader member. 
        /// If the structure is not receiving information about a group, 
        /// this member is ignored.
        /// </summary>
        [Description("Size in TCHARs of the buffer pointed to by the pszHeader member. If the structure is not receiving information about a group, this member is ignored.")]
        public int CchHeader;
        /// <summary>
        /// Pointer to a null-terminated string that contains the footer text 
        /// when item information is being set. If group information is being retrieved, 
        /// this member specifies the address of the buffer that receives the footer text.
        /// </summary>
        [Description("Pointer to a null-terminated string that contains the footer text when item information is being set. If group information is being retrieved, this member specifies the address of the buffer that receives the footer text.")]
        [MarshalAs(UnmanagedType.LPWStr)]
        public string PszFooter;
        /// <summary>
        /// Size in TCHARs of the buffer pointed to by the pszFooter member. 
        /// If the structure is not receiving information about a group, 
        /// this member is ignored.
        /// </summary>
        [Description("Size in TCHARs of the buffer pointed to by the pszFooter member. If the structure is not receiving information about a group, this member is ignored.")]
        public int CchFooter;
        /// <summary>
        /// ID of the group.
        /// </summary>
        [Description("ID of the group.")]
        public int IGroupId;
        /// <summary>
        /// Mask used with LVM_GETGROUPINFO (Microsoft Windows XP and Windows Vista) 
        /// and LVM_SETGROUPINFO (Windows Vista only) to specify which flags in the 
        /// state value are being retrieved or set.
        /// </summary>
        [Description("Mask used with LVM_GETGROUPINFO (Microsoft Windows XP and Windows Vista) and LVM_SETGROUPINFO (Windows Vista only) to specify which flags in the state value are being retrieved or set.")]
        public int StateMask;
        /// <summary>
        /// Flag that can have one of the following values:LVGS_NORMALGroups are expanded, 
        /// the group name is displayed, and all items in the group are displayed.
        /// </summary>
        [Description("Flag that can have one of the following values:LVGS_NORMAL Groups are expanded, the group name is displayed, and all items in the group are displayed.")]
        public ListViewGroupState State;
        /// <summary>
        /// Indicates the alignment of the header or footer text for the group. 
        /// It can have one or more of the following values. Use one of the header flags. 
        /// Footer flags are optional. 
        /// Windows XP: Footer flags are reserved.LVGA_FOOTER_CENTERReserved.
        /// </summary>
        [Description("Indicates the alignment of the header or footer text for the group. It can have one or more of the following values. Use one of the header flags. Footer flags are optional. Windows XP: Footer flags are reserved.LVGA_FOOTER_CENTERReserved.")]
        public uint UAlign;
        /// <summary>
        /// Windows Vista. Pointer to a null-terminated string that contains the 
        /// subtitle text when item information is being set. If group information 
        /// is being retrieved, this member specifies the address of the buffer that 
        /// receives the subtitle text. This element is drawn under the header text.
        /// </summary>
        [Description("Windows Vista. Pointer to a null-terminated string that contains the subtitle text when item information is being set. If group information is being retrieved, this member specifies the address of the buffer that receives the subtitle text. This element is drawn under the header text.")]
        public IntPtr PszSubtitle;
        /// <summary>
        /// Windows Vista. Size, in TCHARs, of the buffer pointed to by the 
        /// pszSubtitle member. If the structure is not receiving information 
        /// about a group, this member is ignored.
        /// </summary>
        [Description("Windows Vista. Size, in TCHARs, of the buffer pointed to by the pszSubtitle member. If the structure is not receiving information about a group, this member is ignored.")]
        public uint CchSubtitle;
        /// <summary>
        /// Windows Vista. Pointer to a null-terminated string that contains the text 
        /// for a task link when item information is being set. If group information 
        /// is being retrieved, this member specifies the address of the buffer 
        /// that receives the task text. This item is drawn right-aligned opposite 
        /// the header text. When clicked by the user, 
        /// the task link generates an LVN_LINKCLICK notification.
        /// </summary>
        [Description("Windows Vista. Pointer to a null-terminated string that contains the text for a task link when item information is being set. If group information is being retrieved, this member specifies the address of the buffer that receives the task text. This item is drawn right-aligned opposite the header text. When clicked by the user, the task link generates an LVN_LINKCLICK notification.")]
        [MarshalAs(UnmanagedType.LPWStr)]
        public string PszTask;
        /// <summary>
        /// Windows Vista. Size in TCHARs of the buffer pointed to by the pszTask member. 
        /// If the structure is not receiving information about a group, 
        /// this member is ignored.
        /// </summary>
        [Description("Windows Vista. Size in TCHARs of the buffer pointed to by the pszTask member. If the structure is not receiving information about a group, this member is ignored.")]
        public uint CchTask;
        /// <summary>
        /// Windows Vista. Pointer to a null-terminated string that contains the 
        /// top description text when item information is being set. 
        /// If group information is being retrieved, this member specifies the address 
        /// of the buffer that receives the top description text. 
        /// This item is drawn opposite the title image when there is a title image, 
        /// no extended image, and uAlign==LVGA_HEADER_CENTER.
        /// </summary>
        [Description("Windows Vista. Pointer to a null-terminated string that contains the top description text when item information is being set. If group information is being retrieved, this member specifies the address of the buffer that receives the top description text. This item is drawn opposite the title image when there is a title image, no extended image, and uAlign==LVGA_HEADER_CENTER.")]
        [MarshalAs(UnmanagedType.LPWStr)]
        public string PszDescriptionTop;
        /// <summary>
        /// Windows Vista. Size in TCHARs of the buffer pointed to by the 
        /// pszDescriptionTop member. If the structure is not receiving information 
        /// about a group, this member is ignored.
        /// </summary>
        [Description("Windows Vista. Size in TCHARs of the buffer pointed to by the pszDescriptionTop member. If the structure is not receiving information about a group, this member is ignored.")]
        public uint CchDescriptionTop;
        /// <summary>
        /// Windows Vista. Pointer to a null-terminated string that contains the 
        /// bottom description text when item information is being set. 
        /// If group information is being retrieved, this member specifies the address 
        /// of the buffer that receives the bottom description text. 
        /// This item is drawn under the top description text when there is a title image, 
        /// no extended image, and uAlign==LVGA_HEADER_CENTER.
        /// </summary>
        [Description("Windows Vista. Pointer to a null-terminated string that contains the bottom description text when item information is being set. If group information is being retrieved, this member specifies the address of the buffer that receives the bottom description text. This item is drawn under the top description text when there is a title image, no extended image, and uAlign==LVGA_HEADER_CENTER.")]
        [MarshalAs(UnmanagedType.LPWStr)]
        public string PszDescriptionBottom;
        /// <summary>
        /// Windows Vista. Size in TCHARs of the buffer pointed to by the 
        /// pszDescriptionBottom member. If the structure is not receiving 
        /// information about a group, this member is ignored.
        /// </summary>
        [Description("Windows Vista. Size in TCHARs of the buffer pointed to by the pszDescriptionBottom member. If the structure is not receiving information about a group, this member is ignored.")]
        public uint CchDescriptionBottom;
        /// <summary>
        /// Windows Vista. Index of the title image in the control imagelist.
        /// </summary>
        [Description("Windows Vista. Index of the title image in the control imagelist.")]
        public int ITitleImage;
        /// <summary>
        /// Windows Vista. Index of the extended image in the control imagelist.
        /// </summary>
        [Description("Windows Vista. Index of the extended image in the control imagelist.")]
        public int IExtendedImage;
        /// <summary>
        /// Windows Vista. Read-only.
        /// </summary>
        [Description("Windows Vista. Read-only.")]
        public int IFirstItem;
        /// <summary>
        /// Windows Vista. Read-only in non-owner data mode.
        /// </summary>
        [Description("Windows Vista. Read-only in non-owner data mode.")]
        public IntPtr CItems;
        /// <summary>
        /// Windows Vista. NULL if group is not a subset. 
        /// Pointer to a null-terminated string 
        /// that contains the subset title text when item information is being set. 
        /// If group information is being retrieved, this member specifies the address 
        /// of the buffer that receives the subset title text.
        /// </summary>
        [Description("Windows Vista. NULL if group is not a subset. Pointer to a null-terminated string that contains the subset title text when item information is being set. If group information is being retrieved, this member specifies the address of the buffer that receives the subset title text.")]
        public IntPtr PszSubsetTitle;
        /// <summary>
        /// Windows Vista. Size in TCHARs of the buffer pointed to by the 
        /// pszSubsetTitle member. 
        /// If the structure is not receiving information about a group, 
        /// this member is ignored.
        /// </summary>
        [Description("Windows Vista. Size in TCHARs of the buffer pointed to by the pszSubsetTitle member. If the structure is not receiving information about a group, this member is ignored.")]
        public IntPtr CchSubsetTitle;
    }

    public enum ListViewGroupMask
    {
        None = 0x00000,
        Header = 0x00001,
        Footer = 0x00002,
        State = 0x00004,
        Align = 0x00008,
        GroupId = 0x00010,
        SubTitle = 0x00100,
        Task = 0x00200,
        DescriptionTop = 0x00400,
        DescriptionBottom = 0x00800,
        TitleImage = 0x01000,
        ExtendedImage = 0x02000,
        Items = 0x04000,
        Subset = 0x08000,
        SubsetItems = 0x10000
    }

    public enum ListViewGroupState
    {
        /// <summary>
        /// Groups are expanded, the group name is displayed, 
        /// and all items in the group are displayed.
        /// </summary>
        Normal = 0,
        /// <summary>
        /// The group is collapsed.
        /// </summary>
        Collapsed = 1,
        /// <summary>
        /// The group is hidden.
        /// </summary>
        Hidden = 2,
        /// <summary>
        /// Version 6.00 and Windows Vista. The group does not display a header.
        /// </summary>
        NoHeader = 4,
        /// <summary>
        /// Version 6.00 and Windows Vista. The group can be collapsed.
        /// </summary>
        Collapsible = 8,
        /// <summary>
        /// Version 6.00 and Windows Vista. The group has keyboard focus.
        /// </summary>
        Focused = 16,
        /// <summary>
        /// Version 6.00 and Windows Vista. The group is selected.
        /// </summary>
        Selected = 32,
        /// <summary>
        /// Version 6.00 and Windows Vista. The group displays only a portion of its items.
        /// </summary>
        SubSeted = 64,
        /// <summary>
        /// Version 6.00 and Windows Vista. The subset link of the group has keyboard focus.
        /// </summary>
        SubSetLinkFocused = 128,
    }
}
