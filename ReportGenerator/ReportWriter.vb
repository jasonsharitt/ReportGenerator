Imports System.IO
Imports System.Text

Public Class ReportWriter
    Public Function WriteCSV(ByVal filename As String, ByVal reportname As String, ByVal saveFileLocation As String, ByVal filedictionary As Dictionary(Of String, List(Of String)), _
                              ByVal filedictionary2 As Dictionary(Of String, List(Of String)), rawfilelist As List(Of String)) As String
        'This function writes out the data contained in the DataGridView to a .csv file that is named
        'according to the report run. The function writes out a header, then loops through the data
        'creating a standard .csv file that can be used in Excel for analyzing.

        'For RepeatNonRepeat (the Contiguous Track report) and RepeatsPerXMinute reports, this function
        'keeps track of the files it creates and passes that information to another function that combines them into one full day report. 


        Dim str As String = saveFileLocation + Path.GetFileName(filename) & reportname + ".csv"
        Dim streamwriter As New System.IO.StreamWriter(str)
        Dim header As String = Form1.DataGridView1.Columns(0).HeaderText
        Dim filepiece As String = Path.GetFileName(filename)
        Dim CTdate As String = filepiece.Substring(0, 10)
        Dim headercount As Integer = Form1.DataGridView1.Columns.Count - 1
        For i As Integer = 1 To headercount
            header += "," + Form1.DataGridView1.Columns(i).HeaderText
        Next
        streamwriter.WriteLine(header)
        Dim rowcount As Integer = Form1.DataGridView1.Rows.Count - 1
        For m As Integer = 0 To rowcount
            Dim strrowvalue As StringBuilder = New StringBuilder
            strrowvalue.Append(CStr(Form1.DataGridView1.Rows(m).Cells(0).Value))
            For n As Integer = 1 To Form1.DataGridView1.Columns.Count - 1
                If Form1.DataGridView1.CurrentCell.Value Is DBNull.Value Then
                    strrowvalue.Append("," + "0")
                End If
                If Form1.DataGridView1.Rows(m).Cells(n).Value Is DBNull.Value Then
                    strrowvalue.Append("," + "0")
                Else
                    strrowvalue.Append("," + CStr(Form1.DataGridView1.Rows(m).Cells(n).Value))
                End If
            Next
            strrowvalue = strrowvalue.Replace(vbCr, "")
            If m <> rowcount Then
                streamwriter.WriteLine(strrowvalue)
            End If
            strrowvalue.Clear()
        Next
        streamwriter.Close()
        If Form1.CheckBox1.Checked Then
            If str.Contains("Repeats_NonRepeats") Then
                If Not filedictionary.ContainsKey(CTdate) Then
                    filedictionary.Add(CTdate, New List(Of String)(New String() {str}))
                End If
                Dim dictkeys = filedictionary(CTdate)
                If Not dictkeys.Contains(str) Then
                    dictkeys.Add(str)
                End If
            End If
        End If
        If Form1.CheckBox2.Checked Then
            If str.Contains("Minutes") Then
                If Not filedictionary2.ContainsKey(CTdate) Then
                    filedictionary2.Add(CTdate, New List(Of String)(New String() {str}))
                End If
                Dim dictkeys = filedictionary2(CTdate)
                If Not dictkeys.Contains(str) Then
                    dictkeys.Add(str)
                End If
            End If
        End If
        If Form1.CheckBox5.Checked Then
            If Not rawfilelist.Contains(str) Then
                rawfilelist.Add(str)
            End If
        End If
        Return True
    End Function
End Class
