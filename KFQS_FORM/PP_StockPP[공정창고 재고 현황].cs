﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using DC00_assm;
using DC00_WinForm;
using DC00_PuMan;
using Infragistics.Win;
using Infragistics.Win.UltraWinGrid;
using DC_POPUP;

/**********************************************
* FORM ID      : PP_StockPP
* FORM NAME    : 공정 재고 조회 및 원자재 입고 등록 취소
* DATE         : 2022-07-06
* MAKER        : 백두산
* 
* 수정사항     :
* ********************************************/

namespace KFQS_Form
{
    
    public partial class PP_StockPP : DC00_WinForm.BaseMDIChildForm
    {
        #region < MEMBER AREA >

        DataTable dtTemp            = new DataTable();
        UltraGridUtil GridUtil      = new UltraGridUtil();       // 그리드 세팅 클래스
        private string sPlantCode   = LoginInfo.PlantCode;       // 로그인한 공장 정보




        public PP_StockPP()
        {
            InitializeComponent();
        }

        private void PP_StockPP_Load(object sender, EventArgs e)
        {
            // Grid 세팅

            
            GridUtil.InitializeGrid(this.grid1);

            GridUtil.InitColumnUltraGrid(grid1, "CHK"           , "자재출고취소"    , GridColDataType_emu.CheckBox    , 100, HAlign.Center, true, true);
            GridUtil.InitColumnUltraGrid(grid1, "PLANTCODE"     , "공장"            , GridColDataType_emu.VarChar     , 130, HAlign.Left  , true, false);
            GridUtil.InitColumnUltraGrid(grid1, "ITEMCODE"      , "품목코드"        , GridColDataType_emu.VarChar     , 130, HAlign.Left  , true, false);
            GridUtil.InitColumnUltraGrid(grid1, "ITEMNAME"      , "품목명"          , GridColDataType_emu.VarChar     , 130, HAlign.Left  , true, false);
            GridUtil.InitColumnUltraGrid(grid1, "ITEMTYPE"      , "품목구분"        , GridColDataType_emu.VarChar     , 130, HAlign.Left  , true, false);
            GridUtil.InitColumnUltraGrid(grid1, "MATLOTNO"      , "LOTNO"           , GridColDataType_emu.VarChar     , 130, HAlign.Left  , true, false);
            GridUtil.InitColumnUltraGrid(grid1, "WHCODE"        , "입고창고"        , GridColDataType_emu.VarChar     , 130, HAlign.Left  , true, false);
            GridUtil.InitColumnUltraGrid(grid1, "STOCKQTY"      , "재고수량"        , GridColDataType_emu.Double      , 130, HAlign.Right , true, false);
            GridUtil.InitColumnUltraGrid(grid1, "UNITCODE"      , "단위"            , GridColDataType_emu.VarChar     , 130, HAlign.Left  , true, false);
            GridUtil.InitColumnUltraGrid(grid1, "MAKER"         , "생성자"          , GridColDataType_emu.VarChar     , 130, HAlign.Left  , true, false);
            GridUtil.InitColumnUltraGrid(grid1, "MAKEDATE"      , "생성일시"        , GridColDataType_emu.DateTime24  , 160, HAlign.Left  , true, false);


            GridUtil.SetInitUltraGridBind(grid1);  // 엔터키를 안쳐도 데이터를 입력한 것으로 인식한다.

            // 콤보박스 세팅

            #region < 콤보박스 세팅 설명 >

            // 공장

            // 공장에 대한 시스템 공통 코드 내역을 DB에서 가져온다. => dtTemp 데이터 테이블에 담는다.
            dtTemp = Common.StandardCODE("PLANTCODE");

            // 콤보박스에 받아온 데이터를 밸류멤버와 디스플레이멤버로 표현
            Common.FillComboboxMaster(this.cboPlantCode_H, dtTemp);

            // 그리드의 컬럼에 받아온 데이터를 콤보박스 형식으로 세팅
            UltraGridUtil.SetComboUltraGrid(grid1, "PLANTCODE", dtTemp);

            #endregion 

            // 품목 타입 ( FERT : 완제품, ROH : 원자재, HALB : 반제품, OM : 외주가공품)
            dtTemp = Common.StandardCODE("ITEMTYPE");
            Common.FillComboboxMaster(cboItemType_H, dtTemp);
            UltraGridUtil.SetComboUltraGrid(grid1, "ITEMTYPE", dtTemp);

            // POP UP
            BizTextBoxManager txtPopMan = new BizTextBoxManager();
            txtPopMan.PopUpAdd(txtItemCode_H, txtItemName_H, "ITEM_MASTER");

            // 단위
            dtTemp = Common.StandardCODE("UNITCODE");
            UltraGridUtil.SetComboUltraGrid(grid1, "UNITCODE", dtTemp);

            // 창고
            dtTemp = Common.StandardCODE("WHCODE");
            UltraGridUtil.SetComboUltraGrid(grid1, "WHCODE", dtTemp);
                   

            // 공장 로그인 정보로 자동 세팅
            cboPlantCode_H.Value = sPlantCode;

        }
        #endregion

        #region < Toolbar Area >
        public override void DoInquire()
        {
            // 툴바의 조회버튼 클릭
            DBHelper helper = new DBHelper(false);

            try
            {
                string sPlantCode_  = Convert.ToString(cboPlantCode_H.Value);   // 공장          
                string sLotNO       = Convert.ToString(txtLOTNO_H.Text);        // 창고번호
                string sItemCode    = Convert.ToString(txtItemCode_H.Text);     // 품목코드
                string sItemName    = Convert.ToString(txtItemName_H.Text);     // 품목명
                string sItemType    = Convert.ToString(cboItemType_H.Value);    // 품목유형
                

                dtTemp = helper.FillTable("09PP_StockPP_S", CommandType.StoredProcedure 
                                          ,helper.CreateParameter("@PLANTCODE"      , sPlantCode_ )
                                          ,helper.CreateParameter("@MATLOTNO"       , sLotNO)
                                          ,helper.CreateParameter("@ITEMCODE"       , sItemCode)
                                          ,helper.CreateParameter("@ITEMNAME"       , sItemName)
                                          ,helper.CreateParameter("@ITEMTYPE"       , sItemType)
                                          );
                
                // 받아온 데이터 그리드 매핑
                if (dtTemp.Rows.Count == 0)
                {
                    // 그리드 초기화
                    GridUtil.Grid_Clear(grid1);
                    ShowDialog("조회할 테이터가 없습니다.", DialogForm.DialogType.OK);
                    return;
                }

                grid1.DataSource = dtTemp;
                grid1.DataBinds(dtTemp);

            }
            catch (Exception ex)
            {
                ShowDialog(ex.ToString());
            }
            finally
            {
                helper.Close();
            }
        }


        #endregion

        /* private void btnLabelPrint_Click(object sender, EventArgs e)
         {
             // 자재 식별표 출력

             // 1. 식별표에 전달할 데이터 등록
             if (grid1.Rows.Count == 0) return;
             // 그리드 컬럼을 그대로 가지고 있는 빈깡통 데이터 행 생성
             DataRow drRow = ((DataTable)grid1.DataSource).NewRow();
             // 선택한 행에 있는 데이터를 빈깡통 행에 등록
             drRow["ITEMCODE"] = Convert.ToString(grid1.ActiveRow.Cells["ITEMCODE"].Value);
             drRow["ITEMNAME"] = Convert.ToString(grid1.ActiveRow.Cells["ITEMNAME"].Value);
             drRow["STOCKQTY"] = Convert.ToString(grid1.ActiveRow.Cells["STOCKQTY"].Value);
             drRow["MATLOTNO"] = Convert.ToString(grid1.ActiveRow.Cells["MATLOTNO"].Value);
             drRow["UNITCODE"] = Convert.ToString(grid1.ActiveRow.Cells["UNITCODE"].Value);

             // 바코드 디자인 객체 선언
             Report_LotBacode lotBarcode = new Report_LotBacode();
             // 바코드 디자인을 담을 레포트 북 객체 선언
             Telerik.Reporting.ReportBook reportBook = new Telerik.Reporting.ReportBook();
             // 바코드 디자인에 데이터 바인딩
             lotBarcode.DataSource = drRow;
             // 데이터가 바인딩 된 레포트 디자인 레포트 북에 담기
             reportBook.Reports.Add(lotBarcode);

             // 레포트 뷰어엥 레보프 북 추가하여 미리보기 창 활성화
             ReportViewer reportView = new ReportViewer(reportBook, 1);
             reportView.ShowDialog();

         }*/

        public override void DoSave()
        {
            
            // 1. 그리드에서 변경된 내역이 있는 행만 추출
            DataTable dt = grid1.chkChange();
            if (dt == null) return;

            DBHelper helper = new DBHelper("", true);

            try
            {
                // 변경 내역을 저장 하시겠습니까?
                if (ShowDialog("변경 내역을 저장하시겠습니까?") == DialogResult.Cancel) return;

                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    if (Convert.ToString(dt.Rows[i]["CHK"]) == "0") continue;
                    if (Convert.ToString(dt.Rows[i]["ITEMTYPE"]) != "ROH") throw new Exception("원자재가 아닌 LOT는 원자재 출고 취소/환입을 할 수 없습니다.");

                    helper.ExecuteNoneQuery("09PP_StockOut_U", CommandType.StoredProcedure
                                            ,helper.CreateParameter("@PLANTCODE" , Convert.ToString(dt.Rows[i]["PLANTCODE"]))
                                            ,helper.CreateParameter("@LOTNO"     , Convert.ToString(dt.Rows[i]["LOTNO"]))
                                            ,helper.CreateParameter("@ITEMCODE"  , Convert.ToString(dt.Rows[i]["ITEMCODE"]))
                                            ,helper.CreateParameter("@QTY"       , Convert.ToString(dt.Rows[i]["STOCKQTY"]))
                                            ,helper.CreateParameter("@UNITCODE"  , Convert.ToString(dt.Rows[i]["UNITCODE"]))
                                            ,helper.CreateParameter("@WORKERID"  , LoginInfo.UserID)
                                            );
                    if (helper.RSCODE != "S") throw new Exception("생산 출고 등록 중 오류가 발생했습니다.");

                }

                helper.Commit();
                DoInquire();
                ShowDialog("정상적으로 등록했습니다.",DialogForm.DialogType.OK);
                
            }
            catch (Exception ex)
            {
                helper.Rollback();
                MessageBox.Show(ex.ToString());
            }
            finally
            {
                helper.Close();
            }

        }


    }
}
