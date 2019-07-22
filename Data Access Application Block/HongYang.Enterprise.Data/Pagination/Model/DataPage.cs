using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HongYang.Enterprise.Data
{
    public class DataPage
    {
        /// <summary>
        /// 当前页
        /// </summary>
        private int currentPage;
        public int CurrentPage
        {
            get
            {
                return this.currentPage;
            }
            set
            {
                currentPage = value;
            }
        }

        /// <summary>
        /// 每页显示数
        /// </summary>
        private int pageSize;
        public int PageSize
        {
            get
            {
                return this.pageSize;
            }
            set
            {
                pageSize = value;
            }
        }

        /// <summary>
        /// 总数
        /// </summary>
        private int totalCount;
        public int TotalCount
        {
            get
            {
                return this.totalCount;
            }
            set
            {
                totalCount = value;
            }
        }

        /// <summary>
        /// 起始点
        /// </summary>
        public int StartPageIndex
        {
            get
            {
                return currentPage * pageSize;
            }
        }

        /// <summary>
        /// 终止点
        /// </summary>
        public int EndPageIndex
        {
            get
            {
                return (currentPage + 1) * pageSize;
            }
        }

        /// <summary>
        /// 总页数
        /// </summary>
        private int totalPageCount;
        public int TotalPageCount
        {
            get
            {
                return pageSize == 0 ? 0 : totalCount / pageSize + (totalCount % pageSize > 0 ? 1 : 0);
            }
            set
            {
                totalCount = value;
            }
        }

    }
}
