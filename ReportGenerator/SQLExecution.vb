Imports System.Data.SqlClient

Public Class SQLExecution
    Dim ds As New DataSet
    Public Function SQLCommander(ByVal cmdtxt As String, ByVal inttime As String, ByVal distance As String, ByVal sequenceregion As String)
        Dim sql2 As New SqlCommand()
        Dim connectionstring As String = "Data Source=(local)\SQLExpress,34524;Initial Catalog=Analysis;Integrated Security=True;Connect Timeout=60"
        Dim connection As New SqlConnection(connectionstring)
        Try
            sql2 = New SqlCommand
            With sql2
                .CommandTimeout = 0
                .CommandText = cmdtxt
                If cmdtxt.Contains("RepeatsPerXMinutes") Then
                    .Parameters.Add("@intervaltime", SqlDbType.NVarChar, 30).Value = inttime
                ElseIf cmdtxt.Contains("CoordinateJumpCount") Then
                    .Parameters.Add("@JumpDistance", SqlDbType.NVarChar, 10).Value = distance
                ElseIf cmdtxt.Contains("DroppedPackets") Then
                    .Parameters.Add("@sequenceNumRegion", SqlDbType.NVarChar, 2).Value = sequenceregion
                End If
                .CommandType = CommandType.StoredProcedure
                .Connection = connection
                .Connection.Open()
            End With
            Dim sda As New SqlDataAdapter(sql2)
            Try
                ds = New DataSet()
                sda.Fill(ds)
                Form1.DataGridView1.DataSource = ds.Tables(0)
                Form1.DataGridView1.Refresh()
            Catch ex2 As Exception
                MsgBox(ex2.ToString)
            End Try
        Catch ex1 As Exception
            MsgBox(ex1.ToString)
        End Try
        sql2.Parameters.Clear()
        sql2.Connection.Close()
        Return True
    End Function
End Class
