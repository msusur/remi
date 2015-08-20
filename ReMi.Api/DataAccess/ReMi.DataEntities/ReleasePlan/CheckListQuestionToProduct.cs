using ReMi.DataEntities.Products;

namespace ReMi.DataEntities.ReleasePlan
{
    public class CheckListQuestionToProduct
    {
        #region scalar props

        public int CheckListQuestionsToProductsId { get; set; }

        public int ProductId { get; set; }

        public int CheckListQuestionId { get; set; }

        #endregion

        #region navigational props

        public virtual CheckListQuestion CheckListQuestion { get; set; }

        public virtual Product Product  { get; set; }
        
        #endregion

        public override string ToString()
        {
            return string.Format("[ProductId={0}, CheckListQuestionId={1}, CheckListQuestionToProductId={2}]", ProductId,
                CheckListQuestionId, CheckListQuestionsToProductsId);
        }
    }
}
