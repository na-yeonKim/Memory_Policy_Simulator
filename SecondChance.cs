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
        public List<List<Page>> frameSnapshots = new List<List<Page>>();

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
            Page newPage = new Page
            {
                pid = Page.CREATE_ID++,
                data = data,
                reference = false
            };

            bool found = frame_window.Any(x => x.data == data);

            if (found) // HIT
            {
                this.hit++;

                int count = frame_window.Count;
                for (int i = 0; i < count; i++)
                {
                    Page page = frame_window.Dequeue();
                    if (page.data == data)
                    {
                        page.reference = true;

                        Page hitPage = new Page
                        {
                            pid = Page.CREATE_ID++,
                            data = data,
                            status = Page.STATUS.HIT,
                            reference = true,
                            loc = count
                        };
                        pageHistory.Add(hitPage);

                        frame_window.Enqueue(page);
                        frameSnapshots.Add(frame_window.ToList());
                        return hitPage.status;
                    }
                    frame_window.Enqueue(page);
                }
            }
            else // PAGEFAULT or MIGRATION
            {
                if (frame_window.Count >= p_frame_size) // MIGRATION
                {
                    newPage.status = Page.STATUS.MIGRATION;

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
                pageHistory.Add(newPage);
                frameSnapshots.Add(frame_window.ToList());
                return newPage.status;
            }

            return newPage.status;
        }
    }
}