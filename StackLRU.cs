using System;
using System.Collections.Generic;
using System.Linq;

namespace Memory_Policy_Simulator
{
    class StackLRU
    {
        private int cursor;
        public int p_frame_size;
        public LinkedList<Page> frame_window;
        public List<Page> pageHistory;

        public int hit;
        public int fault;
        public int migration;

        public StackLRU(int get_frame_size)
        {
            this.cursor = 0;
            this.p_frame_size = get_frame_size;
            this.frame_window = new LinkedList<Page>();
            this.pageHistory = new List<Page>();
        }

        public Page.STATUS Operate(char data)
        {
            Page newPage = new Page();

            bool found = frame_window.Any(x => x.data == data);

            if (found) // HIT
            {
                this.hit++;

                var node = frame_window.First;
                while (node != null)
                {
                    if (node.Value.data == data)
                    {
                        Page page = node.Value;
                        frame_window.Remove(node);
                        page.reference = true;
                        frame_window.AddLast(page); // top으로 이동

                        Page hitPage = new Page
                        {
                            pid = Page.CREATE_ID++,
                            data = data,
                            status = Page.STATUS.HIT,
                            reference = true,
                            loc = frame_window.Count
                        };
                        pageHistory.Add(hitPage);
                        return hitPage.status;
                    }
                    node = node.Next;
                }
            }
            else // PAGE FAULT or MIGRATION
            {
                newPage.pid = Page.CREATE_ID++;
                newPage.data = data;

                if (frame_window.Count >= p_frame_size) // MIGRATION
                {
                    newPage.status = Page.STATUS.MIGRATION;
                    frame_window.RemoveFirst();
                    cursor = p_frame_size;
                    this.migration++;
                    this.fault++;
                }
                else // PAGE FAULT
                {
                    newPage.status = Page.STATUS.PAGEFAULT;
                    cursor++;
                    this.fault++;
                }

                newPage.loc = cursor;
                frame_window.AddLast(newPage);
                pageHistory.Add(newPage);
            }

            return newPage.status;
        }

        public List<Page> GetPageInfo(Page.STATUS status)
        {
            List<Page> pages = new List<Page>();
            foreach (Page page in pageHistory)
            {
                if (page.status == status)
                {
                    pages.Add(page);
                }
            }
            return pages;
        }
    }
}