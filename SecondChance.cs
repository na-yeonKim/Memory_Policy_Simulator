using System;
using System.Collections.Generic;
using System.Linq;

namespace Memory_Policy_Simulator
{
    class SecondChance
    {
        private int cursor;
        public int p_frame_size;
        public Queue<Page> frame_window;
        public List<Page> pageHistory;

        public int hit;
        public int fault;
        public int migration;

        public SecondChance(int get_frame_size)
        {
            this.cursor = 0;
            this.p_frame_size = get_frame_size;
            this.frame_window = new Queue<Page>();
            this.pageHistory = new List<Page>();
        }

        public Page.STATUS Operate(char data)
        {
            Page newPage = new Page();

            bool found = frame_window.Any(x => x.data == data);

            if (found) // HIT
            {
                newPage.pid = Page.CREATE_ID++;
                newPage.data = data;
                newPage.status = Page.STATUS.HIT;
                newPage.reference = true;
                this.hit++;

                int i = 0;
                foreach (var x in frame_window)
                {
                    if (x.data == data) break;
                    i++;
                }
                newPage.loc = i + 1;
            }
            else // PAGEFAULT or MIGRATION
            {
                newPage.pid = Page.CREATE_ID++;
                newPage.data = data;
                newPage.reference = true;

                if (frame_window.Count >= p_frame_size) // MIGRATION
                {
                    newPage.status = Page.STATUS.MIGRATION;

                    // 페이지 선택
                    while (true)
                    {
                        Page front = frame_window.Peek();
                        if (front.reference)
                        {
                            front.reference = false;
                            frame_window.Dequeue();
                            frame_window.Enqueue(front);
                        }
                        else
                        {
                            frame_window.Dequeue();
                            break;
                        }
                    }

                    cursor = p_frame_size;
                    this.migration++;
                    this.fault++;
                }
                else // PAGEFAULT
                {
                    newPage.status = Page.STATUS.PAGEFAULT;
                    cursor++;
                    this.fault++;
                }

                newPage.loc = cursor;
                frame_window.Enqueue(newPage);
            }

            pageHistory.Add(newPage);
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