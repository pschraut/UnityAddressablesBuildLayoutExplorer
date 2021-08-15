//
// Addressables Build Layout Explorer for Unity. Copyright (c) 2021 Peter Schraut (www.console-dev.de). See LICENSE.md
// https://github.com/pschraut/UnityAddressablesBuildLayoutExplorer
//
using System;
using System.Collections.Generic;

namespace Oddworm.EditorFramework.BuildLayoutExplorer
{
    public class NavigationBookmark
    {
        public NavigationBookmark()
        {
        }
    }

    public class NavigationCommand
    {
        public BuildLayoutView view;
        public NavigationBookmark bookmark;
        public object target;

        public NavigationCommand()
        {
        }
    }

    public sealed class NavigationHistory
    {
        int m_Index;
        List<Entry> m_Entries = new List<Entry>();

        class Entry
        {
            public NavigationCommand from;
            public NavigationCommand to;
        }

        public NavigationCommand current
        {
            get;
            private set;
        }

        public void Add(NavigationCommand from, NavigationCommand to)
        {
            while (m_Entries.Count > m_Index && m_Index >= 0)
                m_Entries.RemoveAt(m_Entries.Count - 1);

            var e = new Entry();
            e.from = from;
            e.to = to;
            m_Entries.Add(e);
            current = to;
            m_Index++;
        }

        public void Clear()
        {
            m_Index = -1;
            m_Entries.Clear();
            current = null;
        }

        public bool HasBack()
        {
            var i = m_Index - 1;
            if (i < 0)
                return false;
            return true;
        }

        public NavigationCommand Back()
        {
            if (!HasBack())
                return null;

            m_Index--;
            current = m_Entries[m_Index].from;
            return current;
        }

        public bool HasForward()
        {
            var i = m_Index;
            if (i >= m_Entries.Count)
                return false;
            return true;
        }

        public NavigationCommand Forward()
        {
            if (!HasForward())
                return null;

            var command = m_Entries[m_Index];
            m_Index++;
            current = command.to;
            return current;
        }
    }
}
