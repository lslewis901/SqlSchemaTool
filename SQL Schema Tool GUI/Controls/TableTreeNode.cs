using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using System.Text;

namespace Lewis.SST.Controls
{
    public class TableTreeNode : ExtTreeNode
    {
        private string _server;
        private string _database;
        private string _table;

        public TableTreeNode()
        {
            // default ctor
        }

        public TableTreeNode(string Text)
        {
            this.Text = Text;
        }

        public string FullTablePath
        {
            get { return "[" + Server + "].[" + Database + "].dbo.[" + Table + "]"; }
        }

        public string Database
        {
            get 
            {
                _database = this.Parent != null ? this.Parent.Text : _database;
                return _database;
            }
        }

        public string Server
        {
            get 
            {
                _server = this.Parent != null && this.Parent.Parent != null ? this.Parent.Parent.Text : _server;
                return _server;
            }
        }

        public string Table
        {
            get 
            {
                _table = this.Text;
                return _table; 
            }
        }

        public bool SelectedForCompare
        {
            get { return this.Checked; }
        }
    }

    public class TableTreeEventArgs : EventArgs
    {
        public TableTreeNode TableTreeNode;
    }

    public class TableTreeNodes : CollectionBase
    {
        public TableTreeNodes ()
        {
            // default ctor
        }

        public virtual int Add(TableTreeNode _tableTreeNode)
        {
            return (this[getIndex(_tableTreeNode)] == null) ? List.Add(_tableTreeNode) : getIndex(_tableTreeNode);
        }

        public virtual TableTreeNode this[int Index]
        {
            get 
            {
                TableTreeNode retval = null;
                if (List.Count > 0 && Index > -1 && Index < List.Count)
                {
                    if (!string.IsNullOrEmpty(((TableTreeNode)List[Index]).Text))
                    {
                        retval = (TableTreeNode)List[Index];
                    }
                }
                return retval;
            }
            set 
            {
                if (List.Count >= 0 && Index > -1)
                {
                    if (Index < List.Count)
                    {
                        List.RemoveAt(Index);
                        List.Insert(Index, value);
                    }
                    else if (Index == List.Count)
                    {
                        List.Add(value);
                    }
                    else
                    {
                        for (int ii = List.Count; ii < Index; ii++)
                        {
                            List.Add(new TableTreeNode());
                        }
                        List.Add(value);
                    }
                }
            }
        }

        public virtual int getIndex(TableTreeNode _tableTreeNode)
        {
            return List.IndexOf(_tableTreeNode);
        }

        public virtual void Remove(TableTreeNode _tableTreeNode)
        {
            if (this[getIndex(_tableTreeNode)] != null)
            {
                List.Remove(_tableTreeNode);
            }
        }

        public virtual void InsertAt(int index, TableTreeNode _tableTreeNode)
        {
            List.Insert(index, _tableTreeNode);
        }

        /// <summary>
        /// Only returns TableTreeNodes that are not considered nulls 
        /// </summary>
        public virtual TableTreeNode[] Tables
        {
            get
            {
                TableTreeNode[] ttn = new TableTreeNode[0];
                if (List.Count > 0)
                {
                    int count = 0;
                    for (int ii = 0; ii < List.Count; ii++)
                    {
                        if (!string.IsNullOrEmpty(((TableTreeNode)List[ii]).Text))
                        {
                            count++;
                        }
                    }
                    ttn = new TableTreeNode[count];
                    int zz = 0;
                    for (int ii = 0; ii < List.Count; ii++)
                    {
                        if (!string.IsNullOrEmpty(((TableTreeNode)List[ii]).Text))
                        {
                            ttn[zz] = (TableTreeNode)List[ii];
                            zz++;
                        }
                    }
                }
                return ttn;
            }
        }
    }
}
