using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ServerCommon
{
    public interface IHadKeyNodeBase<T>
    {
        T Key();
    }

    public class QuickSearchLink<K, V> where V : IHadKeyNodeBase<K>
    {
        public void Clear()
        {
            m_dataLinkList.Clear();
            m_indexDic.Clear();
        }

        public int Count
        {
            get
            {
                return m_dataLinkList.Count;
            }
        }

        public LinkedListNode<V> AddLast(V data)
        {
            if (m_indexDic.ContainsKey(data.Key()))
                return null;

            LinkedListNode<V> dataNode = m_dataLinkList.AddLast(data);
            m_indexDic.Add(data.Key(), dataNode);

            return dataNode;
        }

        public LinkedListNode<V> AddLast(LinkedListNode<V> node)
        {
            if (m_indexDic.ContainsKey(node.Value.Key()))
                return null;

            m_dataLinkList.AddLast(node);
            m_indexDic.Add(node.Value.Key(), node);

            return node;
        }

        public LinkedListNode<V> AddFirst(V data)
        {
            if (m_indexDic.ContainsKey(data.Key()))
                return null;

            LinkedListNode<V> dataNode = m_dataLinkList.AddFirst(data);
            m_indexDic.Add(data.Key(), dataNode);

            return dataNode;
        }

        public LinkedListNode<V> AddFirst(LinkedListNode<V> node)
        {
            if (m_indexDic.ContainsKey(node.Value.Key()))
                return null;

            m_dataLinkList.AddFirst(node);
            m_indexDic.Add(node.Value.Key(), node);

            return node;
        } 

        public LinkedListNode<V> PeekFirst()
        {
            if (m_dataLinkList.Count == 0)
                return null;

            return m_dataLinkList.First;
        }

        public LinkedListNode<V> PeekLast()
        {
            if (m_dataLinkList.Count == 0)
                return null;

            return m_dataLinkList.Last;
        }

        public LinkedListNode<V> PopFirst()
        {
            if (m_dataLinkList.Count == 0)
                return null;

            LinkedListNode<V> first = m_dataLinkList.First;
            m_dataLinkList.RemoveFirst();
            m_indexDic.Remove(first.Value.Key());
            return first;
        }

        public LinkedListNode<V> PopLast()
        {
            if (m_dataLinkList.Count == 0)
                return null;

            LinkedListNode<V> last = m_dataLinkList.Last;
            m_dataLinkList.RemoveLast();
            m_indexDic.Remove(last.Value.Key());
            return last;
        }

        public LinkedListNode<V> PeekData(K key)
        {
            LinkedListNode<V> node;
            if (m_indexDic.TryGetValue(key, out node))
                return node;
            return null;
        }

        public LinkedListNode<V> PopData(K key)
        {
            LinkedListNode<V> node;
            if (m_indexDic.TryGetValue(key, out node))
            {
                m_indexDic.Remove(key);
                m_dataLinkList.Remove(node);
                return node;
            }
            return null;
        }

        public LinkedListNode<V> AddBefore(LinkedListNode<V> posNode, V data)
        {
            if (m_indexDic.ContainsKey(data.Key()))
                return null;

            LinkedListNode<V> node = m_dataLinkList.AddBefore(posNode, data);
            m_indexDic.Add(data.Key(), node);
            return node;
        }

        public LinkedListNode<V> AddBefore(LinkedListNode<V> posNode, LinkedListNode<V> insertNode)
        {
            if(m_indexDic.ContainsKey(insertNode.Value.Key()))
                return null;

            m_dataLinkList.AddBefore(posNode, insertNode);
            m_indexDic.Add(insertNode.Value.Key(), insertNode);
            return insertNode;
        }


        public LinkedListNode<V> AddAfter(LinkedListNode<V> posNode, V data)
        {
            if (m_indexDic.ContainsKey(data.Key()))
                return null;

            LinkedListNode<V> node = m_dataLinkList.AddAfter(posNode, data);
            m_indexDic.Add(data.Key(), node);
            return node;
        }

        public LinkedListNode<V> AddAfter(LinkedListNode<V> posNode, LinkedListNode<V> insertNode)
        {
            if (m_indexDic.ContainsKey(insertNode.Value.Key()))
                return null;

            m_dataLinkList.AddAfter(posNode, insertNode);
            m_indexDic.Add(insertNode.Value.Key(), insertNode);
            return insertNode;
        }

        public LinkedList<V> DataLink
        {
            get
            {
                return m_dataLinkList;
            }
        }

        private LinkedList<V> m_dataLinkList = new LinkedList<V>();
        private Dictionary<K, LinkedListNode<V>> m_indexDic = new Dictionary<K, LinkedListNode<V>>();
    }

}