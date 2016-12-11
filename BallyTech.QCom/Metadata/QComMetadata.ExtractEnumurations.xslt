<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform" xmlns:QCom="mailto:jhantin@ballytech.com?Subject=QComMetadata" xmlns:gen="mailto:jhantin@ballytech.com?Subject=ExternalMessageCodeGenerator">
  <xsl:template match="/">
    <gen:enum name="MeterCodes" type="System.Byte">
      <xsl:for-each select="QCom:QComMetadata/QCom:MeterDefinitions/QCom:MeterDefinition">
        <gen:value name="{@Name}" value="{@Code}"/>
      </xsl:for-each>
    </gen:enum>
  </xsl:template>
</xsl:stylesheet>