Imports System.Environment
Imports System.Reflection
Imports System.IO
Class MainWindow
#Disable Warning CA2101 ' Specify marshaling for P/Invoke string arguments
    Private Declare Function GetPrivateProfileString Lib "kernel32" Alias "GetPrivateProfileStringA" (lpApplicationName As String, lpKeyName As String, lpDefault As String, lpReturnedString As String, nSize As Integer, lpFileName As String) As Integer
#Disable Warning CA2101 ' Specify marshaling for P/Invoke string arguments
    Private Declare Function WritePrivateProfileString Lib "kernel32" Alias "WritePrivateProfileStringA" (lpApplicationName As String, lpKeyName As String, lpString As String, lpFileName As String) As Integer
    Public Shared Function GetINI(Section As String, AppName As String, lpDefault As String, FileName As String) As String
        Dim Str As String = ""
        Str = LSet(Str, 256)
        GetPrivateProfileString(Section, AppName, lpDefault, Str, Len(Str), FileName)
        Return Microsoft.VisualBasic.Left(Str, InStr(Str, Chr(0)) - 1)
    End Function
    Public Shared Function WriteINI(Section As String, AppName As String, lpDefault As String, FileName As String) As Long
        WriteINI = WritePrivateProfileString(Section, AppName, lpDefault, FileName)
    End Function
    Dim CurrentAPI As String
    ReadOnly Cache As String = GetEnvironmentVariable("LOCALAPPDATA") + "\Borderlands 3\Saved"
    ReadOnly Path As String = GetFolderPath（SpecialFolder.MyDocuments） + "\My Games\Borderlands 3\Saved\Config\WindowsNoEditor\GameUserSettings.ini"
    ReadOnly configPath As String = GetFolderPath（SpecialFolder.MyDocuments） + "\My Games\Borderlands 3\Saved\Config\WindowsNoEditor\Engine.ini"
    Private Sub MainWindow_Loaded(sender As Object, e As RoutedEventArgs) Handles Me.Loaded
        'MsgBox(String.Join(",", Assembly.GetExecutingAssembly().GetManifestResourceNames().ToArray()))
        RefreshAPI()
        CalculateCache()
        DetectConfig()
    End Sub
    Private Sub CalculateCache()
        Dim FileSize As ULong
        Dim di As New DirectoryInfo(Cache)
        Try
            For Each fi In di.GetFiles()
                FileSize += fi.Length()
            Next
            ClearCache.Content = "Clear Cache:" + FormatBytes(FileSize)
            ClearCache.IsEnabled = True
        Catch
            ClearCache.Content = "No Cache Found"
        End Try
    End Sub
    Private Sub DetectConfig()
        If File.Exists(configPath) Then
            Dim fInfo As New FileInfo(configPath)
            If fInfo.IsReadOnly Then
                PatchConfig.Content = "Already Patched"
            Else
                PatchConfig.Content = "Patch Now"
                PatchConfig.IsEnabled = True
            End If
        Else
            PatchConfig.Content = "Patch Now"
            PatchConfig.IsEnabled = True
        End If
    End Sub

    Dim DoubleBytes As Double
    Public Function FormatBytes(BytesCaller As ULong) As String
        Try
            Select Case BytesCaller
                Case Is >= 1099511627776
                    DoubleBytes = BytesCaller / 1099511627776 'TB
                    Return FormatNumber(DoubleBytes, 2) & " TB"
                Case 1073741824 To 1099511627775
                    DoubleBytes = BytesCaller / 1073741824 'GB
                    Return FormatNumber(DoubleBytes, 2) & " GB"
                Case 1048576 To 1073741823
                    DoubleBytes = BytesCaller / 1048576 'MB
                    Return FormatNumber(DoubleBytes, 2) & " MB"
                Case 1024 To 1048575
                    DoubleBytes = BytesCaller / 1024 'KB
                    Return FormatNumber(DoubleBytes, 2) & " KB"
                Case 0 To 1023
                    DoubleBytes = BytesCaller ' bytes
                    Return FormatNumber(DoubleBytes, 2) & " bytes"
                Case Else
                    Return ""
            End Select
        Catch
            Return ""
        End Try
    End Function
    Private Sub RefreshAPI()
        CurrentAPI = GetINI("/Script/OakGame.OakGameUserSettings", "PreferredGraphicsAPI", "", Path)
        If CurrentAPI = "DX11" Then
            DX11.Background = New SolidColorBrush(ColorConverter.ConvertFromString("#FF44DD20"))
            DX12.Background = New SolidColorBrush(ColorConverter.ConvertFromString("#FFDDDDDD"))
        Else
            DX12.Background = New SolidColorBrush(ColorConverter.ConvertFromString("#FF44DD20"))
            DX11.Background = New SolidColorBrush(ColorConverter.ConvertFromString("#FFDDDDDD"))
        End If
    End Sub

    Private Sub DX11_Click(sender As Object, e As RoutedEventArgs) Handles DX11.Click
        WriteINI("/Script/OakGame.OakGameUserSettings", "PreferredGraphicsAPI", "DX11", Path)
        RefreshAPI()
    End Sub

    Private Sub DX12_Click(sender As Object, e As RoutedEventArgs) Handles DX12.Click
        WriteINI("/Script/OakGame.OakGameUserSettings", "PreferredGraphicsAPI", "DX12", Path)
        RefreshAPI()
    End Sub

    Private Sub ClearCache_Click(sender As Object, e As RoutedEventArgs) Handles ClearCache.Click
        Try
            Directory.Delete(Cache, True)
            ClearCache.Content = "Cache Cleared!"
            ClearCache.Background = New SolidColorBrush(ColorConverter.ConvertFromString("#FF44DD20"))
            ClearCache.IsEnabled = False
        Catch

        End Try
    End Sub

    Private Async Sub PatchConfig_Click(sender As Object, e As RoutedEventArgs) Handles PatchConfig.Click
        Dim asm As Assembly = Assembly.GetExecutingAssembly()
        If File.Exists(configPath) Then
            File.Delete(configPath)
        End If
        Dim fileStream = File.Create(configPath)
        Await asm.GetManifestResourceStream("Borderlands3Helper.Engine.ini").CopyToAsync(fileStream)
        fileStream.Close()
        Dim fInfo As New FileInfo(configPath)
        fInfo.IsReadOnly = True
        PatchConfig.Content = "Patch Success"
        PatchConfig.IsEnabled = False
    End Sub

    Private Sub Github_MouseDown(sender As Object, e As MouseButtonEventArgs) Handles Github.MouseDown
        Dim startInfo As New ProcessStartInfo("https://github.com/GlacierLab/Borderlands3Helper")
        startInfo.UseShellExecute = True
        Process.Start(startInfo)
    End Sub
End Class
