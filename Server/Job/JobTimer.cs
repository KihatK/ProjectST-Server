using ServerCore;
namespace Server.Job {
    /// <summary>
    /// 우선순위 큐를 적용하기 위한 element 설정
    /// </summary>
    struct JobTimerElem : IComparable<JobTimerElem> {
        public int execTick;
        public IJob job;

        public int CompareTo(JobTimerElem other) {
            return other.execTick - execTick;
        }
    }

    /// <summary>
    /// 특정 시간 후에 실행시킬 Job을 순서대로 보관 및 실행해주는 객체
    /// </summary>
    class JobTimer {
        PriorityQueue<JobTimerElem> _pq = new PriorityQueue<JobTimerElem>();
        object _lock = new object();

        /// <summary>
        /// 특정 시간 후에 실행시킬 Job 삽입
        /// </summary>
        /// <param name="job">실행시킬 job</param>
        /// <param name="tickAfter">몇 초 뒤에 실행시킬건지 넣기</param>
        public void Push(IJob job, int tickAfter = 0) {
            JobTimerElem jobElement;
            jobElement.execTick = Environment.TickCount + tickAfter;
            jobElement.job = job;

            lock (_lock) {
                _pq.Push(jobElement);
            }
        }

        /// <summary>
        /// 주기적으로 확인을 하여 실행시킬 시간이 되었다면 실행
        /// </summary>
        public void Flush() {
            while (true) {
                int now = Environment.TickCount;

                JobTimerElem jobElement;

                lock (_lock) {
                    if (_pq.Count == 0) {
                        break;
                    }

                    jobElement = _pq.Peek();
                    if (jobElement.execTick > now) {
                        break;
                    }

                    _pq.Pop();
                }
                jobElement.job.Execute();
            }
        }
    }
}
