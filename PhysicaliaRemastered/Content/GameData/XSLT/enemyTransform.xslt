<?xml version="1.0"?> 
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform"> 
	<xsl:template match="/">
		<AssetLibrary libraryType="Enemy">
			<xsl:apply-templates select="//Enemy" />
		</AssetLibrary>
	</xsl:template>
	<xsl:template match="Enemy">
		<xsl:element name="Asset">
			<xsl:attribute name="key">
				<xsl:value-of select="@typeID" />
			</xsl:attribute>
			<xsl:attribute name="textureId">
				<xsl:value-of select="Animations/Animation[@action='1']/@key" />
			</xsl:attribute>
			<xsl:attribute name="textureType">Animation</xsl:attribute>
		</xsl:element>
	</xsl:template>
</xsl:stylesheet>