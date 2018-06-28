using System;

namespace ADO.Extensions.DBContext
{
    public class DBResult
    {
        public DBResult()
        {
        }

        public DBResult(bool sucess, int rowAffected)
        {
            this.Success = sucess;
            this.RowAffected = rowAffected;
        }

        public DBResult(bool sucess, string errorMessage)
        {
            this.Success = sucess;
            this.ErrorMessage = errorMessage;
        }

        public DBResult(bool sucess, int rowAffected, string errorMessage)
        {
            this.Success = sucess;
            this.RowAffected = rowAffected;
            this.ErrorMessage = errorMessage;
        }        

        public bool Success { get; private set; }
        public int RowAffected { get; private set; }
        public String ErrorMessage { get; private set; }
    }
}