targetScope = 'resourceGroup'

param name string
param tags object
param workspaceResourceId string
param applicationInsightsResourceId string
param workspaceAssociationName string
param applicationInsightsAssociationName string

resource scope 'Microsoft.Insights/privateLinkScopes@2021-09-01' = {
  name: name
  location: 'global'
  tags: tags
  properties: {
    accessModeSettings: {
      ingestionAccessMode: 'PrivateOnly'
      queryAccessMode: 'PrivateOnly'
    }
  }
}

resource workspaceAssociation 'Microsoft.Insights/privateLinkScopes/scopedResources@2021-09-01' = {
  parent: scope
  name: workspaceAssociationName
  properties: {
    linkedResourceId: workspaceResourceId
  }
}

resource insightsAssociation 'Microsoft.Insights/privateLinkScopes/scopedResources@2021-09-01' = {
  parent: scope
  name: applicationInsightsAssociationName
  properties: {
    linkedResourceId: applicationInsightsResourceId
  }
}

output resourceId string = scope.id
