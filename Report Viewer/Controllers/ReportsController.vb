Imports System.Data.SqlClient
Imports System.IO
Imports System.Web.Mvc
Imports CrystalDecisions.CrystalReports.Engine
Imports CrystalDecisions.Shared
Imports Newtonsoft.Json
Imports ReportViewer.Models

Public Class ReportsController
    Inherits Controller

    Private ReadOnly _connectionString As String = GetConnectionString()
    Public Shared connectionInfo As New ConnectionInfo()

    ' GET: /Reports/EnterParameters
    Public Function EnterParameters() As ActionResult
        Return View()
    End Function

    ' POST: /Reports/ViewReport
    <HttpPost>
    Public Function ViewReport() As ActionResult
        Try
            ' Read JSON data from the request body
            Dim jsonData As String
            Using reader As New StreamReader(Request.InputStream)
                jsonData = reader.ReadToEnd()
            End Using

            ' Log incoming JSON data
            System.Diagnostics.Debug.WriteLine("Received JSON: " & jsonData)

            ' Deserialize JSON data into ReportRequest object
            Dim reportRequest As ReportRequest = JsonConvert.DeserializeObject(Of ReportRequest)(jsonData)

            ' Create a new ReportDocument instance
            Dim reportDocument As New ReportDocument()

            ' Load the Crystal Report file
            Dim reportPath As String = Server.MapPath($"~/Reports/{reportRequest.report?.reportFile}")
            If Not System.IO.File.Exists(reportPath) Then
                Throw New FileNotFoundException("The report file was not found.", reportPath)
            End If
            reportDocument.Load(reportPath)

            ' Update report connection info
            UpdateReportConnectionInfo(reportDocument, connectionInfo)

            ' Refresh the report data
            reportDocument.Refresh()

            ' Set report parameters
            If reportRequest.reportData.parameters IsNot Nothing Then

                For Each parameter In reportRequest.reportData.parameters
                    Try
                        ' Set parameter values
                        Dim paramName = parameter.name.TrimStart("@"c)

                        reportDocument.SetParameterValue(parameter.name, parameter.value)
                        System.Diagnostics.Debug.WriteLine($"Set parameter: {parameter.name} = {parameter.value}")
                    Catch ex As Exception
                        System.Diagnostics.Debug.WriteLine($"Error setting parameter '{parameter.name}': {ex.Message}")
                    End Try
                Next

            End If

            ' Set the response format
            Response.Buffer = False
            Response.Clear()
            Response.ContentType = "application/pdf"
            Response.AddHeader("content-disposition", $"inline; filename={reportRequest.ExportFile?.ExportFileName}.pdf")

            ' Export the report to PDF
            Using stream As Stream = reportDocument.ExportToStream(ExportFormatType.PortableDocFormat)
                If stream Is Nothing Then
                    Throw New Exception("The report could not be exported to PDF.")
                End If
                stream.Seek(0, SeekOrigin.Begin)
                stream.CopyTo(Response.OutputStream)
            End Using

            ' Cleanup
            reportDocument.Close()
            reportDocument.Dispose()

            ' Ensure the response is completed
            Response.Flush()
            Response.SuppressContent = True

            Return New EmptyResult()
        Catch ex As Exception
            ' Log the error and return a user-friendly message
            System.Diagnostics.Debug.WriteLine("An error occurred while generating the report: " & ex.Message)
            System.Diagnostics.Debug.WriteLine("Stack Trace: " & ex.StackTrace)
            Response.Clear()
            Response.ContentType = "text/plain"
            Response.Write("An error occurred while generating the report: " & ex.Message)
            Response.End()
            Return New EmptyResult()
        End Try
    End Function

    Private Shared Sub UpdateReportConnectionInfo(report As ReportDocument, connectionInfo As ConnectionInfo)
        Try
            Dim tables As Tables = report.Database.Tables
            For Each table As CrystalDecisions.CrystalReports.Engine.Table In tables
                Dim tableLogonInfo As TableLogOnInfo = table.LogOnInfo
                tableLogonInfo.ConnectionInfo = connectionInfo
                table.ApplyLogOnInfo(tableLogonInfo)
            Next
            System.Diagnostics.Debug.WriteLine("Report connection info updated successfully.")
        Catch ex As Exception
            System.Diagnostics.Debug.WriteLine($"Error in UpdateReportConnectionInfo: {ex.Message}")
        End Try
    End Sub

    Private Shared Function GetConnectionString() As String
        Try
            Dim configFilePath As String = System.Web.Hosting.HostingEnvironment.MapPath("~/App_Data/DatabaseConfig.json")
            If configFilePath Is Nothing OrElse Not System.IO.File.Exists(configFilePath) Then
                Throw New FileNotFoundException("Configuration file not found.", configFilePath)
            End If

            Dim jsonConfig As String = System.IO.File.ReadAllText(configFilePath)
            Dim dbConfig As DatabaseConfig = JsonConvert.DeserializeObject(Of DatabaseConfig)(jsonConfig)
            If dbConfig?.database?.authentication?.username Is Nothing OrElse dbConfig?.database?.authentication?.password Is Nothing Then
                Throw New Exception("Database credentials are missing in the configuration.")
            End If

            Dim server As String = dbConfig.database.server
            Dim dbName As String = dbConfig.database.databaseName
            Dim user As String = dbConfig.database.authentication.username
            Dim password As String = dbConfig.database.authentication.password

            connectionInfo.ServerName = server
            connectionInfo.DatabaseName = dbName
            connectionInfo.UserID = user
            connectionInfo.Password = password
            Return $"Server={server};Database={dbName};User Id={user};Password={password};"
        Catch ex As Exception
            System.Diagnostics.Debug.WriteLine("An error occurred while constructing the connection string: " & ex.Message)
            Throw
        End Try
    End Function

End Class


Public Class ReportDetails
    Public Property reportformat As String
    Public Property reportFile As String
End Class

Public Class ExportDetails
    Public Property format As String
    Public Property ExportFileName As String
    Public Property ExportFilePath As String
    Public Property SendAllstoragePath As Boolean
    Public Property DeleteAfterDate As Integer
End Class




Public Class DatabaseSettings
    Public Property server As String
    Public Property databaseName As String
    Public Property authentication As AuthSettings
End Class

Public Class AuthSettings
    Public Property username As String
    Public Property password As String
End Class
