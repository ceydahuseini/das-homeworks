<%@ Page Title="О нас" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="About.aspx.cs" Inherits="WebApplication2.About" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">
    <main class="about-container" aria-labelledby="title">
        <h2 id="title" class="about-title">За оваа апликација</h2>
        <h3 class="about-subtitle">Дизајн на софтвер и архитектура</h3>

        <p>Оваа апликација ви овозможува динамички да преземате, прикажувате и извезувате податоци за акции од Македонската берза (МБ).</p>

        <p><strong>Клучни функции:</strong></p>
        <ul>
            <li>Преземање на податоци за акции во реално време од МБ</li>
            <li>Избор на издавачи преку паѓачка листа</li>
            <li>Прикажување на податоци за акции во чиста, одговорна табела</li>
            <li>Извоз на податоци во CSV формат</li>
        </ul>

        <p><strong>Користени технологии:</strong></p>
        <ul>
            <li>C# (ASP.NET)</li>
            <li>HTML, CSS (Респонзивен дизајн)</li>
            <li>HtmlAgilityPack (Веб скрапинг)</li>
        </ul>

        <p>Овој проект е дизајниран со принципи на софтверска архитектура за да се обезбеди скалабилност, одржливост и лесно користење.</p>

       

    </main>
</asp:Content>
