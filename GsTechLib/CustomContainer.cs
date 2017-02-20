using System;
using System.Collections.Generic;
using System.Text;

namespace GsTechLib
{
    //快速链表排序顺序
    public enum eQuickLinkedSortOrder
    {
        None = 0,           //无
        Ascending = 1,      //升序
        Descending = 2,     //降序
    }

    /// <summary>
    /// 快速链表
    ///     链表里面的对象是唯一的，同一对象不能重复出现在链表中，同一结点更不能出现在链表中
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class QuickLinkedList<T> where T : IComparable
    {
        public QuickLinkedList(eQuickLinkedSortOrder sortOrder)
        {
            m_sortOrder = sortOrder;
        }

        public int Count
        {
            get
            {
                return m_linkList.Count;
            }
        }

        public LinkedListNode<T> First
        {
            get
            {
                return m_linkList.First;
            }
        }

        public LinkedListNode<T> Last
        {
            get
            {
                return m_linkList.Last;
            }
        }

        public LinkedListNode<T> AddFirst(T value)
        {
            if (checkValueIn(value))
                return null;

            LinkedListNode<T> node = new LinkedListNode<T>(value);
            m_linkList.AddFirst(node);
            m_objHashDic.Add(value.GetHashCode(), node);
            m_nodeHashDic.Add(node.GetHashCode(), node);

            return node;
        }

        public LinkedListNode<T> AddFirst(LinkedListNode<T> node)
        {
            if (checkNodeIn(node))
                return null;

            m_linkList.AddFirst(node);
            m_objHashDic.Add(node.Value.GetHashCode(), node);
            m_nodeHashDic.Add(node.GetHashCode(), node);

            return node;
        }

        public LinkedListNode<T> AddLast(T value)
        {
            if (checkValueIn(value))
                return null;

            LinkedListNode<T> node = new LinkedListNode<T>(value);
            m_linkList.AddLast(node);
            m_objHashDic.Add(value.GetHashCode(), node);
            m_nodeHashDic.Add(node.GetHashCode(), node);

            return node;
        }

        public LinkedListNode<T> AddLast(LinkedListNode<T> node)
        {
            if (checkNodeIn(node))
                return null;

            m_linkList.AddLast(node);
            m_objHashDic.Add(node.Value.GetHashCode(), node);
            m_nodeHashDic.Add(node.GetHashCode(), node);

            return node;
        }

        public LinkedListNode<T> AddBefore(LinkedListNode<T> node, LinkedListNode<T> newNode)
        {
            if (checkNodeIn(newNode))
                return null;

            m_linkList.AddBefore(node, newNode);
            m_objHashDic.Add(newNode.Value.GetHashCode(), newNode);
            m_nodeHashDic.Add(newNode.GetHashCode(), newNode);

            return newNode;
        }

        public LinkedListNode<T> AddBefore(LinkedListNode<T> node, T newValue)
        {
            if (checkValueIn(newValue))
                return null;

            LinkedListNode<T> newNode = new LinkedListNode<T>(newValue);
            m_linkList.AddBefore(node, newNode);
            m_objHashDic.Add(newNode.Value.GetHashCode(), newNode);
            m_nodeHashDic.Add(newNode.GetHashCode(), newNode);

            return newNode;
        }

        public LinkedListNode<T> AddAfter(LinkedListNode<T> node, LinkedListNode<T> newNode)
        {
            if (checkNodeIn(newNode))
                return null;

            m_linkList.AddAfter(node, newNode);
            m_objHashDic.Add(newNode.Value.GetHashCode(), newNode);
            m_nodeHashDic.Add(newNode.GetHashCode(), newNode);

            return newNode;
        }

        public LinkedListNode<T> AddAfter(LinkedListNode<T> node, T newValue)
        {
            if (checkNodeIn(node))
                return null;

            LinkedListNode<T> newNode = new LinkedListNode<T>(newValue);
            m_linkList.AddAfter(node, newNode);
            m_objHashDic.Add(newNode.Value.GetHashCode(), newNode);
            m_nodeHashDic.Add(newNode.GetHashCode(), newNode);

            return newNode;
        }

        public void Clear()
        {
            m_linkList.Clear();
            m_nodeHashDic.Clear();
            m_objHashDic.Clear();
        }

        public bool Contains(T value)
        {
            return m_objHashDic.ContainsKey(value.GetHashCode());
        }

        public bool Contains(LinkedListNode<T> node)
        {
            return m_nodeHashDic.ContainsKey(node.GetHashCode());
        }

        public LinkedListNode<T> Find(T value)
        {
            LinkedListNode<T> node;
            m_objHashDic.TryGetValue(value.GetHashCode(), out node);
            return node;
        }

        public LinkedListNode<T> Remove(T value)
        {
            LinkedListNode<T> node = null;
            int objHash = value.GetHashCode();
            if (m_objHashDic.TryGetValue(objHash, out node) == false)
                return node;

            m_objHashDic.Remove(objHash);
            m_nodeHashDic.Remove(node.GetHashCode());
            m_linkList.Remove(node);
            return node;
        }

        public void Remove(LinkedListNode<T> node)
        {
            if (m_nodeHashDic.TryGetValue(node.GetHashCode(), out node))
            {
                m_nodeHashDic.Remove(node.GetHashCode());
                m_objHashDic.Remove(node.Value.GetHashCode());
                m_linkList.Remove(node);
            }
        }

        public void RemoveFirst()
        {
            if(m_linkList.First != null)
            {
                m_nodeHashDic.Remove(m_linkList.First.GetHashCode());
                m_objHashDic.Remove(m_linkList.First.Value.GetHashCode());
                m_linkList.RemoveFirst();
            }
        }

        public void RemoveLast()
        {
            if(m_linkList.Last != null)
            {
                m_nodeHashDic.Remove(m_linkList.Last.GetHashCode());
                m_objHashDic.Remove(m_linkList.Last.Value.GetHashCode());
                m_linkList.RemoveLast();
            }
        }

        public bool ValueChange4Sort(T value)
        {
            LinkedListNode<T> node;
            if (m_objHashDic.TryGetValue(value.GetHashCode(), out node) == false)
                return false;

            sort(node);
            return true;
        }

        public bool NodeChange4Sort(LinkedListNode<T> node)
        {
            if (m_nodeHashDic.ContainsKey(node.GetHashCode()) == false)
                return false;

            sort(node);
            return true;
        }

        /// <summary>
        /// 检测值是否已经在里面
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        private bool checkValueIn(T value)
        {
            return m_objHashDic.ContainsKey(value.GetHashCode());
        }

        /// <summary>
        /// 检测节点是否在里面
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        private bool checkNodeIn(LinkedListNode<T> node)
        {
            return m_nodeHashDic.ContainsKey(node.GetHashCode());
        }

        /// <summary>
        /// 排序
        /// </summary>
        /// <param name="node"></param>
        private void sort(LinkedListNode<T> node)
        {
            switch (m_sortOrder)
            {
                case eQuickLinkedSortOrder.None:
                    break;
                case eQuickLinkedSortOrder.Ascending:
                    {
                        LinkedListNode<T> preNode = node.Previous;
                        bool chgPos = false;
                        while (preNode != null)
                        {
                            if (preNode.Value.CompareTo(node.Value) <= 0)
                                break;

                            preNode = preNode.Previous;
                            chgPos = true;
                        }
                        if(chgPos == true)
                        {
                            Remove(node);
                            if(preNode == null)
                                AddFirst(node);
                            else
                                AddAfter(preNode, node);
                        }
                        else
                        {
                            LinkedListNode<T> nextNode = node.Next;
                            while(nextNode != null)
                            {
                                if(nextNode.Value.CompareTo(node.Value) >= 0)
                                    break;

                                nextNode = nextNode.Next;
                                chgPos = true;
                            }
                            if(chgPos == true)
                            {
                                Remove(node);
                                if(nextNode == null)
                                    AddLast(node);
                                else 
                                    AddBefore(nextNode, node);
                            }
                        }
                    }
                    break;
                case eQuickLinkedSortOrder.Descending:
                    {
                        LinkedListNode<T> preNode = node.Previous;
                        bool chgPos = false;
                        while (preNode != null)
                        {
                            if (preNode.Value.CompareTo(node.Value) >= 0)
                                break;

                            preNode = preNode.Previous;
                            chgPos = true;
                        }
                        if(chgPos == true)
                        {
                            Remove(node);
                            if(preNode == null)
                                AddFirst(node);
                            else
                                AddAfter(preNode, node);
                        }
                        else
                        {
                            LinkedListNode<T> nextNode = node.Next;
                            while(nextNode != null)
                            {
                                if(nextNode.Value.CompareTo(node.Value) <= 0)
                                    break;

                                nextNode = nextNode.Next;
                                chgPos = true;
                            }
                            if(chgPos == true)
                            {
                                Remove(node);
                                if(nextNode == null)
                                    AddLast(node);
                                else 
                                    AddBefore(nextNode, node);
                            }
                        }
                    }
                    break;
                default:
                    break;
            }
        }

        private eQuickLinkedSortOrder m_sortOrder = eQuickLinkedSortOrder.None;                                     //排序规则
        private LinkedList<T> m_linkList = new LinkedList<T>();                                                     //对象列表
        private Dictionary<int, LinkedListNode<T>> m_nodeHashDic = new Dictionary<int, LinkedListNode<T>>();        //节点HASH索引字典
        private Dictionary<int, LinkedListNode<T>> m_objHashDic = new Dictionary<int, LinkedListNode<T>>();         //对象HASH索引字典
    }
}
