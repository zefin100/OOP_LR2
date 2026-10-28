<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
  <xsl:output method="html" indent="yes" encoding="utf-8"/>

  <xsl:template match="/">
    <html>
      <head>
        <meta charset="utf-8"/>
        <title>Звіт: Програмне забезпечення факультету</title>
        <style>
          body {
            font-family: -apple-system, BlinkMacSystemFont, "Segoe UI", Roboto, Helvetica, Arial, sans-serif;
            background-color: #f8fafc;
            color: #1e293b;
            margin: 20px;
            padding: 0;
          }
          .container {
            max-width: 1100px;
            margin: 0 auto;
            background: #ffffff;
            padding: 30px;
            border-radius: 12px;
            box-shadow: 0 4px 6px -1px rgba(0, 0, 0, 0.1), 0 2px 4px -2px rgba(0, 0, 0, 0.1);
          }
          h1 {
            color: #0f172a;
            border-bottom: 2px solid #3b82f6;
            padding-bottom: 12px;
            margin-top: 0;
            font-size: 24px;
          }
          .subtitle {
            color: #64748b;
            font-size: 14px;
            margin-bottom: 25px;
          }
          .dept-section {
            margin-bottom: 30px;
          }
          .dept-title {
            background-color: #eff6ff;
            color: #1d4ed8;
            padding: 10px 16px;
            border-radius: 8px;
            font-size: 18px;
            font-weight: 600;
            margin-bottom: 15px;
            border-left: 4px solid #3b82f6;
          }
          table {
            width: 100%;
            border-collapse: collapse;
            margin-bottom: 15px;
            font-size: 14px;
          }
          th {
            background-color: #f1f5f9;
            color: #475569;
            text-align: left;
            padding: 12px;
            font-weight: 600;
            border-bottom: 2px solid #cbd5e1;
          }
          td {
            padding: 12px;
            border-bottom: 1px solid #e2e8f0;
            vertical-align: top;
          }
          tr:hover {
            background-color: #f8fafc;
          }
          .badge {
            display: inline-block;
            padding: 3px 8px;
            border-radius: 6px;
            font-size: 12px;
            font-weight: 500;
          }
          .badge-category {
            background-color: #dbeafe;
            color: #1e40af;
          }
          .badge-license {
            background-color: #dcfce7;
            color: #166534;
          }
          .badge-version {
            background-color: #f3e8ff;
            color: #6b21a8;
          }
          .path {
            font-family: monospace;
            font-size: 12px;
            color: #64748b;
            word-break: break-all;
          }
          .footer {
            margin-top: 30px;
            padding-top: 15px;
            border-top: 1px solid #e2e8f0;
            color: #94a3b8;
            font-size: 12px;
            text-align: right;
          }
        </style>
      </head>
      <body>
        <div class="container">
          <h1>Звіт: Програмне забезпечення на мережі факультету</h1>
          <div class="subtitle">
            <xsl:value-of select="/FacultySoftwareNetwork/@university"/> | 
            <xsl:value-of select="/FacultySoftwareNetwork/@faculty"/>
          </div>

          <xsl:for-each select="/FacultySoftwareNetwork/Department">
            <div class="dept-section">
              <div class="dept-title">
                <xsl:value-of select="@name"/> (<xsl:value-of select="@code"/>)
              </div>
              <table>
                <thead>
                  <tr>
                    <th style="width: 25%;">Назва та версія</th>
                    <th style="width: 15%;">Категорія</th>
                    <th style="width: 15%;">Розробник / Ліцензія</th>
                    <th style="width: 45%;">Опис та розташування</th>
                  </tr>
                </thead>
                <tbody>
                  <xsl:for-each select="Software">
                    <tr>
                      <td>
                        <strong><xsl:value-of select="@Name"/></strong>
                        <xsl:if test="@Version">
                          <br/><span class="badge badge-version">v<xsl:value-of select="@Version"/></span>
                        </xsl:if>
                      </td>
                      <td>
                        <xsl:if test="@Category">
                          <span class="badge badge-category"><xsl:value-of select="@Category"/></span>
                        </xsl:if>
                      </td>
                      <td>
                        <xsl:if test="@Author">
                          <div><strong><xsl:value-of select="@Author"/></strong></div>
                        </xsl:if>
                        <xsl:if test="@License">
                          <span class="badge badge-license"><xsl:value-of select="@License"/></span>
                        </xsl:if>
                      </td>
                      <td>
                        <div><xsl:value-of select="Description"/></div>
                        <div class="path" style="margin-top: 6px;">📂 <xsl:value-of select="DistributionLocation"/></div>
                        <xsl:if test="Requirements">
                          <div style="font-size: 12px; color: #64748b; margin-top: 4px;">⚙️ Вимоги: <xsl:value-of select="Requirements"/></div>
                        </xsl:if>
                      </td>
                    </tr>
                  </xsl:for-each>
                </tbody>
              </table>
            </div>
          </xsl:for-each>

          <div class="footer">
            Згенеровано табличним процесором XML | Варіант 9 (37) | Черненко Ігор Ігорович (К-28)
          </div>
        </div>
      </body>
    </html>
  </xsl:template>
</xsl:stylesheet>
