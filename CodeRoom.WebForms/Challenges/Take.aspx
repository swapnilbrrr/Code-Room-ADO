<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Take.aspx.cs" Inherits="CodeRoom.WebForms.Challenges.Take" MasterPageFile="~/Site.Master" %>
<asp:Content ID="cTitle" ContentPlaceHolderID="TitleContent" runat="server">Challenge - Code-Room</asp:Content>
<asp:Content ID="cBody" ContentPlaceHolderID="MainContent" runat="server">
<section class="page-shell"><div class="container challenge-take">
 <asp:Panel ID="NotFoundPanel" runat="server" Visible="false" CssClass="empty-state"><div class="empty-state-icon">🧩</div><h2>Challenge unavailable.</h2><p>The requested practice challenge could not be found.</p></asp:Panel>
 <asp:Panel ID="ChallengePanel" runat="server" Visible="false">
  <article class="content-panel challenge-panel">
   <div class="challenge-header"><div><span class="eyebrow">PRACTICE CHALLENGE · <asp:Literal ID="Category" runat="server" /></span><h1><asp:Literal ID="ChallengeTitle" runat="server" /></h1></div><span class="xp-chip">+<asp:Literal ID="Points" runat="server" /> XP</span></div>
   <div class="challenge-instructions"><asp:Repeater ID="InstructionRepeater" runat="server"><ItemTemplate><p><%#: Container.DataItem %></p></ItemTemplate></asp:Repeater></div>
   <asp:Panel ID="StarterPanel" runat="server" Visible="false"><div class="code-block"><div class="code-block-label">STARTER</div><pre><code><asp:Literal ID="StarterCode" runat="server" /></code></pre></div></asp:Panel>
   <asp:Panel ID="HintPanel" runat="server" Visible="false"><details class="hint-box"><summary>Need a hint?</summary><p><asp:Literal ID="Hint" runat="server" /></p></details></asp:Panel>
   <div class="challenge-answer-form"><label for="Answer">Your answer</label><asp:TextBox ID="Answer" runat="server" CssClass="input challenge-editor" TextMode="MultiLine" Rows="5" placeholder="Type your answer here..." /><asp:HiddenField ID="CsrfToken" runat="server" /><asp:Button ID="SubmitButton" runat="server" CssClass="btn btn-primary" Text="Run check ⚡" OnClick="SubmitButton_Click" CausesValidation="true" /><asp:RequiredFieldValidator ID="AnswerValidator" runat="server" ControlToValidate="Answer" ErrorMessage="Enter an answer before running the check." CssClass="validation-message" Display="Dynamic" />
   </div>
   <a class="text-link" href='<%= ResolveUrl("~/Challenges/Index.aspx?id=") %><%= CourseId %>'>← Back to challenge list</a>
  </article>
 </asp:Panel>
</div></section>
</asp:Content>