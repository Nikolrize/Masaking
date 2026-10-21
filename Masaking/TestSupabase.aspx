<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="TestSupabase.aspx.cs" Inherits="Masaking.TestSupabase" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>Supabase Connection Test</title>
    <meta name="viewport" content="width=device-width, initial-scale=1" />

    <style>
        body {
            font-family: Arial, sans-serif;
            background: #f4f4f9;
            margin: 0;
            padding: 40px 20px;
        }

        .container {
            max-width: 500px;
            margin: 0 auto;
            padding: 30px;
            background: white;
            border-radius: 12px;
            box-shadow: 0 4px 15px rgba(0, 0, 0, 0.08);
        }

        h1 {
            margin-top: 0;
            color: #333;
        }

        .description {
            color: #666;
            line-height: 1.5;
        }

        .test-button {
            background: #7c3aed;
            color: white;
            border: none;
            padding: 12px 18px;
            border-radius: 6px;
            cursor: pointer;
            font-size: 15px;
            margin-top: 15px;
        }

            .test-button:hover {
                background: #6d28d9;
            }

        .result {
            display: block;
            margin-top: 20px;
            padding: 12px;
            border-radius: 6px;
            overflow-wrap: anywhere;
        }
    </style>
</head>

<body>
    <form id="form1" runat="server">
        <div class="container">
            <h1>Supabase Connection Test</h1>

            <p class="description">
                Click the button below to check whether
                Masaking can connect to your Supabase database.
           
            </p>

            <asp:Button
                ID="btnTestConnection"
                runat="server"
                Text="Test Connection"
                CssClass="test-button"
                OnClick="btnTestConnection_Click" />

            <asp:Label
                ID="lblResult"
                runat="server"
                CssClass="result"
                EnableViewState="false" />
        </div>
    </form>
</body>
</html>
