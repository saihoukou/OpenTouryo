﻿'**********************************************************************************
'* Copyright (C) 2007,2014 Hitachi Solutions,Ltd.
'**********************************************************************************

#Region "Apache License"
'
' Licensed to the Apache Software Foundation (ASF) under one or more 
' contributor license agreements. See the NOTICE file distributed with
' this work for additional information regarding copyright ownership. 
' The ASF licenses this file to you under the Apache License, Version 2.0
' (the "License"); you may not use this file except in compliance with 
' the License. You may obtain a copy of the License at
'
' http://www.apache.org/licenses/LICENSE-2.0
'
' Unless required by applicable law or agreed to in writing, software
' distributed under the License is distributed on an "AS IS" BASIS,
' WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
' See the License for the specific language governing permissions and
' limitations under the License.
'
#End Region

'**********************************************************************************
'* クラス名        ：WCFTCPSvcForFx
'* クラス日本語名  ：WCF TCP/IPサービス（サービス インターフェイス基盤）
'*                   .NET言語用のTCP/IPメソッド（.NETオブジェクトI/F）を公開する。
'*
'* 作成日時        ：－
'* 作成者          ：生技
'* 更新履歴        ：
'* 
'*  日時        更新者            内容
'*  ----------  ----------------  -------------------------------------------------
'*  2012/12/17  西野 大介         新規作成
'**********************************************************************************

Imports System.ServiceModel
Imports System.ServiceModel.Activation

' System
Imports System
Imports System.Xml
Imports System.Data
Imports System.Collections

' 業務フレームワーク
Imports Touryo.Infrastructure.Business.Business
Imports Touryo.Infrastructure.Business.Common
Imports Touryo.Infrastructure.Business.Dao
Imports Touryo.Infrastructure.Business.Exceptions
Imports Touryo.Infrastructure.Business.Presentation
Imports Touryo.Infrastructure.Business.Util

' フレームワーク
Imports Touryo.Infrastructure.Framework.Business
Imports Touryo.Infrastructure.Framework.Common
Imports Touryo.Infrastructure.Framework.Dao
Imports Touryo.Infrastructure.Framework.Exceptions
Imports Touryo.Infrastructure.Framework.Presentation
Imports Touryo.Infrastructure.Framework.Util
Imports Touryo.Infrastructure.Framework.Transmission

' 部品
Imports Touryo.Infrastructure.Public.Db
Imports Touryo.Infrastructure.Public.IO
Imports Touryo.Infrastructure.Public.Log
Imports Touryo.Infrastructure.Public.Str
Imports Touryo.Infrastructure.Public.Util

Namespace Touryo.Infrastructure.Business.Transmission
	' メモ: [リファクター] メニューの [名前の変更] コマンドを使用すると、コード、svc、および config ファイルで同時にクラス名 "WCFSvcForFx" を変更できます。

	''' <summary>
	''' WCF TCP/IPサービス（サービス インターフェイス基盤）
	''' .NET言語用のTCP/IPメソッド（.NETオブジェクトI/F）を公開する。
	''' </summary>
	<ServiceBehavior> _
	Public Class WCFTCPSvcForFx
		Implements IWCFTCPSvcForFx
		#Region "グローバル変数"

		''' <summary>インプロセス呼び出しの名前解決シングルトン クラス</summary>
		''' <remarks>
		''' 初期化は起動時の１回のみであり、
		''' 読み取り専用のデータを保持する場合
		''' のみに適用するデザインパターンとする。
		''' </remarks>
		Private Shared IPR_NS As New InProcessNameService()

		#End Region

		''' <summary>
		''' WCF TCP/IPサービスを使用した
		''' サービス インターフェイス基盤（.NETオンライン）
		''' </summary>
		''' <param name="serviceName">サービス名</param>
		''' <param name="contextObject">コンテキスト</param>
		''' <param name="parameterValueObject">引数</param>
		''' <param name="returnValueObject">戻り値</param>
		''' <returns>返すべきエラーの情報</returns>
		''' <remarks>値は全て.NETオブジェクトをバイナリシリアライズしたバイト配列データ</remarks>
        Public Function DotNETOnlineTCP(serviceName As String, ByRef contextObject As Byte(), parameterValueObject As Byte(), ByRef returnValueObject As Byte()) As Byte() Implements IWCFTCPSvcForFx.DotNETOnlineTCP
            ' ステータス
            Dim status As String = "－"

            ' 初期化のため
            returnValueObject = Nothing

            '#Region "呼出し制御関係の変数"

            ' アセンブリ名
            Dim assemblyName As String = ""

            ' クラス名
            Dim className As String = ""

            '#End Region

            '#Region "引数・戻り値関係の変数"

            ' コンテキスト情報
            Dim context As Object
            ' 2009/09/29-この行
            ' 引数・戻り値の.NETオブジェクト
            Dim parameterValue As BaseParameterValue
            Dim returnValue As BaseReturnValue

            ' エラー情報（クライアント側で復元するため）
            Dim wsErrorInfo As New WSErrorInfo()

            ' エラー情報（ログ出力用）
            Dim errorType As String = ""
            ' 2009/09/15-この行
            Dim errorMessageID As String = ""
            Dim errorMessage As String = ""
            Dim errorToString As String = ""

            '#End Region

            Try
                ' 開始ログの出力
                LogIF.InfoLog("SERVICE-IF", FxLiteral.SIF_STATUS_START)

                '#Region "名前解決"

                ' ★
                status = FxLiteral.SIF_STATUS_NAME_SERVICE

                ' 名前解決（インプロセス）
                WCFTCPSvcForFx.IPR_NS.NameResolution(serviceName, assemblyName, className)

                '#End Region

                '#Region "引数のデシリアライズ"

                ' ★
                status = FxLiteral.SIF_STATUS_DESERIALIZE

                ' コンテキストクラスの.NETオブジェクト化
                context = BinarySerialize.BytesToObject(contextObject)
                ' 2009/09/29-この行
                ' ※ コンテキストの利用方法は任意だが、サービスインターフェイス上での利用に止める。
                ' 引数クラスの.NETオブジェクト化
                parameterValue = DirectCast(BinarySerialize.BytesToObject(parameterValueObject), BaseParameterValue)

                ' 引数クラスをパラメタ セットに格納
                Dim paramSet As Object() = New Object() {parameterValue, DbEnum.IsolationLevelEnum.User}

                '#End Region

                '#Region "認証処理のＵＯＣ"

                ' ★
                status = FxLiteral.SIF_STATUS_AUTHENTICATION

                ' ★★　コンテキストの情報を使用するなどして
                '       認証処理をＵＯＣする（必要に応じて）。

                '/ 認証チケットの復号化
                'string[] authTicket = (string[])BinarySerialize.BytesToObject(
                '    CustomEncode.FromBase64String(
                '        SymmetricCryptography.DecryptString(
                '            (string)context, GetConfigParameter.GetConfigValue("private-key"),
                '            EnumSymmetricAlgorithm.TripleDESCryptoServiceProvider)));

                '/ 認証チケットの整合性を確認

                '/ Ｂ層・Ｄ層呼出し
                '/   スライディング・タイムアウトの実装、
                '/   タイムスタンプのチェックと、更新
                'returnValue = (BaseReturnValue)Latebind.InvokeMethod(
                '    "xxxx", "yyyy",
                '    FxLiteral.TRANSMISSION_INPROCESS_METHOD_NAME,
                '    new object[] { new AuthParameterValue("－", "－", "zzzz", "",
                '        ((MyParameterValue)parameterValue).User, authTicket[1]),
                '        DbEnum.IsolationLevelEnum.User });

                'if (returnValue.ErrorFlag)
                '{
                '    // 認証エラー
                '    throw new BusinessSystemException("xxxx", "認証チケットが不正か、タイムアウトです。");
                '}

                ' 持ち回るならCookieにするか、
                ' contextをrefにするなどとする。
                contextObject = BinarySerialize.ObjectToBytes(DateTime.Now)
                ' 更新されたかのテストコード
                '#End Region

                '#Region "Ｂ層・Ｄ層呼出し"

                ' ★
                status = FxLiteral.SIF_STATUS_INVOKE

                ' #17-start
                Try
                    ' Ｂ層・Ｄ層呼出し
                    'returnValue = (BaseReturnValue)Latebind.InvokeMethod(
                    '    AppDomain.CurrentDomain.BaseDirectory + "\\bin\\" + assemblyName + ".dll",
                    '    className, FxLiteral.TRANSMISSION_INPROCESS_METHOD_NAME, paramSet);
                    returnValue = DirectCast(Latebind.InvokeMethod(assemblyName, className, FxLiteral.TRANSMISSION_INPROCESS_METHOD_NAME, paramSet), BaseReturnValue)
                Catch rtEx As System.Reflection.TargetInvocationException
                    ' InnerExceptionを投げなおす。
                    Throw rtEx.InnerException
                End Try
                ' #17-end

                '#End Region

                '#Region "戻り値のシリアライズ"

                ' ★
                status = FxLiteral.SIF_STATUS_SERIALIZE

                returnValueObject = BinarySerialize.ObjectToBytes(returnValue)

                '#End Region

                ' ★
                status = ""

                ' 戻り値を返す。
                Return Nothing
            Catch bsEx As BusinessSystemException
                ' システム例外

                ' エラー情報を設定する。
                wsErrorInfo.ErrorType = FxEnum.ErrorType.BusinessSystemException
                wsErrorInfo.ErrorMessageID = bsEx.messageID
                wsErrorInfo.ErrorMessage = bsEx.Message

                ' ログ出力用の情報を保存
                errorType = FxEnum.ErrorType.BusinessSystemException.ToString()
                ' 2009/09/15-この行
                errorMessageID = bsEx.messageID
                errorMessage = bsEx.Message

                errorToString = bsEx.ToString()

                ' エラー情報を戻す。
                Return BinarySerialize.ObjectToBytes(wsErrorInfo)
            Catch fxEx As FrameworkException
                ' フレームワーク例外
                ' ★ インナーエクセプション情報は消失

                ' エラー情報を設定する。
                wsErrorInfo.ErrorType = FxEnum.ErrorType.FrameworkException
                wsErrorInfo.ErrorMessageID = fxEx.messageID
                wsErrorInfo.ErrorMessage = fxEx.Message

                ' ログ出力用の情報を保存
                errorType = FxEnum.ErrorType.FrameworkException.ToString()
                ' 2009/09/15-この行
                errorMessageID = fxEx.messageID
                errorMessage = fxEx.Message

                errorToString = fxEx.ToString()

                ' エラー情報を戻す。
                Return BinarySerialize.ObjectToBytes(wsErrorInfo)
            Catch ex As Exception
                ' ログ出力用の情報を保存
                errorType = FxEnum.ErrorType.ElseException.ToString()
                ' 2009/09/15-この行
                errorMessageID = "－"
                errorMessage = ex.Message

                errorToString = ex.ToString()

                ' SoapExceptionになって伝播
                Throw
            Finally
                ' Sessionステートレス
                'HttpContext.Current.Session.Clear();
                'HttpContext.Current.Session.Abandon();

                ' 終了ロクの出力
                If status = "" Then
                    ' 終了ログ出力
                    LogIF.InfoLog("SERVICE-IF", "正常終了")
                Else
                    ' 終了ログ出力
                    ' 2009/09/15-この行
                    LogIF.ErrorLog("SERVICE-IF", "異常終了" & "：" & status & vbCr & vbLf & "エラー タイプ：" & errorType & vbCr & vbLf & "エラー メッセージID：" & errorMessageID & vbCr & vbLf & "エラー メッセージ：" & errorMessage & vbCr & vbLf & errorToString & vbCr & vbLf)
                End If
            End Try
        End Function
	End Class
End Namespace
