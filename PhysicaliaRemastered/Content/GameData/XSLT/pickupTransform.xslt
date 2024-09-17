<?xml version="1.0"?> 
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
	<xsl:template match="/">
		<AssetLibrary libraryType="Pickup">
			<xsl:apply-templates select="//Pickup" />
		</AssetLibrary>
	</xsl:template>
	<xsl:template match="Pickup">
		<xsl:element name="Asset">
			<xsl:attribute name="key">
				<xsl:value-of select="@key" />
			</xsl:attribute>
			<xsl:attribute name="textureKey">
				<xsl:value-of select="Sprite/@key" />
			</xsl:attribute>
			<xsl:attribute name="textureType">Sprite</xsl:attribute>
		</xsl:element>
	</xsl:template>
</xsl:stylesheet>