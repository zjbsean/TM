using System;
using System.Collections.Generic;
using System.Text;

namespace ServerCommon
{
    //好友操作类型定义(GDS->GS)
    public class FriendsOperationTypeDef
    {
        public const int Friend_Update_4_Some_One_Give_Iten = 100;
        public const int Friend_Update_4_Some_One_Be_Your_Friend = 101;
        public const int Friend_Update_4_Some_One_You_Add_A_Friend = 102;
        
        public const int Friend_Delete_4_Some_One_Not_Your_Friend = 200;
        public const int Firend_Delete_4_You_Not_Need_The_Friend = 201;

        public const int Apply_Update_You_Apply_To_Some_One = 300;
        public const int Apply_Update_Just_Update = 301;

        public const int Apply_Delete_Some_One_Not_Accept = 400;
        public const int Apply_Delete_Some_One_Accept = 401;

        public const int BeApply_Update_Some_One_Apply = 500;

        public const int BeApply_Delete_You_Accept_Some_One_Apply = 600;
        public const int BeApply_Delete_You_Not_Accept_Some_One_Apply = 601;
        public const int BeApply_Delete_Some_One_Not_Apply_You = 602;
    }
}
