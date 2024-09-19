<?xml version="1.0"?> 
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
	<xsl:template match="/">
		<AssetLibrary libraryType="Weapon">
			<xsl:apply-templates select="//Weapon" />
		</AssetLibrary>
	</xsl:template>
	<xsl:template match="Weapon">
		<xsl:element name="Asset">
			<xsl:attribute name="key">
				<xsl:value-of select="@id" />
			</xsl:attribute>
			<xsl:attribute name="textureId">
				<xsl:value-of select="Graphics/Sprite/@key" />
			</xsl:attribute>
			<xsl:attribute name="textureType">Sprite</xsl:attribute>
		</xsl:element>
	</xsl:template>
</xsl:stylesheet>