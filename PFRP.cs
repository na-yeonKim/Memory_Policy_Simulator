using System;
using System.Collections.Generic;
using System.Linq;

namespace Memory_Policy_Simulator
{
    class PFRP
    {
        private int cursor;
        public int p_frame_size;
        public Queue<Page> frame_window;
        public List<Page> pageHistory;

        public int hit;
        public int fault;
        public int migration;

        private Dictionary<int, int> processPageLimit;
        private Dictionary<int, int> procMemCount;

        public PFRP(int get_frame_size, Dictionary<int, int> processPageLimit)
        {
            this.cursor = 0;
            this.p_frame_size = get_frame_size;
            this.frame_window = new Queue<Page>();
            this.pageHistory = new List<Page>();
            this.processPageLimit = processPageLimit;   // 프로세스별 전체 페이지 개수
            this.procMemCount = new Dictionary<int, int>();   // 프로세스별 메모리에 올라간 페이지 개수
        }

        public Page.STATUS Operate(int pid, char data)
        {
            Page newPage = new Page();
            newPage.pid = pid;
            newPage.data = data;
            newPage.reference = true;

            bool found = false;

            List<Page> pages = frame_window.ToList();
            for (int i = 0; i < pages.Count; i++)
            {
                if (pages[i].data == data && pages[i].pid == pid)
                {
                    found = true;

                    Page temp = pages[i];
                    temp.refCount++;

                    pages[i] = temp;
                    break;
                }

                Page fallback = pages[i];
                fallback.refCount = 1;
                pages[i] = fallback;
            }
            frame_window = new Queue<Page>(pages);

            if (found) // HIT
            {
                newPage.status = Page.STATUS.HIT;
                this.hit++;

                int i = 0;
                foreach (var x in frame_window)
                {
                    if (x.data == data && x.pid == pid) break;
                    i++;
                }
                newPage.loc = i + 1;
            }
            else
            {
                newPage.refCount = 1;

                if (frame_window.Count >= p_frame_size) // MIGRATION
                {
                    newPage.status = Page.STATUS.MIGRATION;

                    double maxRatio = -1;
                    int targetPid = -1;

                    // 비율 계산
                    foreach (var kvp in procMemCount)
                    {
                        int mem = kvp.Value;
                        int total = processPageLimit[kvp.Key];
                        double ratio = (double)mem / total;

                        if (ratio > maxRatio)
                        {
                            maxRatio = ratio;
                            targetPid = kvp.Key;
                        }
                    }

                    bool hasTarget = false;
                    Page targetPage = new Page();
                    int minRefCount = int.MaxValue;

                    // targetPid의 페이지 중에서 참조 횟수가 가장 적은 페이지 선택
                    foreach (var p in frame_window)
                    {
                        if (p.pid == targetPid && p.refCount < minRefCount)
                        {
                            minRefCount = p.refCount;
                            targetPage = p;
                            hasTarget = true;
                        }
                    }

                    // targetPage 삭제
                    if (hasTarget)
                    {
                        frame_window = new Queue<Page>(
                            frame_window.Where(p => !(p.data == targetPage.data && p.pid == targetPage.pid)));
                        procMemCount[targetPage.pid]--;
                    }
                    else
                    {
                        Page removed = frame_window.Dequeue();
                        procMemCount[removed.pid]--;
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

                if (!procMemCount.ContainsKey(pid))
                {
                    procMemCount[pid] = 0;
                }
                procMemCount[pid]++;
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
