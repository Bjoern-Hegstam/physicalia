<?xml version="1.0"?> 
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
	<xsl:template match="/">
		<AssetLibrary libraryType="Tile">
			<xsl:apply-templates select="//Tile" />
		</AssetLibrary>
	</xsl:template>
	<xsl:template match="Tile">
		<xsl:element name="Asset">
			<xsl:attribute name="key">
				<xsl:value-of select="@id" />
			</xsl:attribute>
			<xsl:attribute name="textureId">
				<xsl:value-of select="@textureId" />
			</xsl:attribute>
			<xsl:attribute name="textureType">
				<xsl:value-of select="@textureType" />
			</xsl:attribute>
		</xsl:element>
	</xsl:template>
</xsl:stylesheet>