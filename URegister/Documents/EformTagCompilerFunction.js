function getComponentTagsToSubmission(component, submissionData = data, parentKey = '', isNested = false) {
    const result = [];

    // Log component details for debugging
    console.log(`Processing component: key=${component.key || 'no-key'}, type=${component.type}, tags=${JSON.stringify(component.tags || [])}, parentKey=${parentKey}, hasChildren=${!!(component.components && component.components.length)}, isNested=${isNested}`);

    // Add tags for the current component if it has a key and tags
    if (component.key && component.tags && Array.isArray(component.tags) && component.tags.length > 0) {
        let submissionValue = null;
        try {
            if (component.type === 'container' || component.type === 'datagrid' || component.type === 'canvas' || (component.type === 'panel' && component.tree)) {
                // Handle nested data for containers, datagrids, canvas, or panels with tree: true
                submissionValue = submissionData[component.key] !== undefined ? submissionData[component.key] : null;
                console.log(`Nested component key=${component.key}, value=`, JSON.stringify(submissionValue, null, 2));
            } else if (component.type === 'panel' || component.type === 'fieldset' || component.type === 'columns' || component.type === 'table') {
                // Collect children's submission data for panels, fieldsets, columns, tables
                submissionValue = {};
                const collectChildData = (childComponents, parentData, currentKeyPath = '') => {
                    childComponents.forEach(child => {
                        if (child.key) {
                            const childKey = currentKeyPath ? `${currentKeyPath}.${child.key}` : child.key;
                            let childValue = null;
                            if (isNested && currentKeyPath) {
                                // For nested data, traverse the path
                                childValue = childKey.split('.').reduce((obj, key) => (obj && obj[key] !== undefined ? obj[key] : null), parentData);
                            } else {
                                // For flat data, access directly
                                childValue = parentData[child.key] !== undefined ? parentData[child.key] : null;
                            }
                            console.log(`Child key: ${childKey}, Child value: ${JSON.stringify(childValue)}, Source: ${isNested ? 'nested' : 'flat'}`);
                            if (childValue !== undefined) {
                                submissionValue[child.key] = childValue;
                            }
                        }
                        // Recursively collect data from nested components
                        if (child.components && Array.isArray(child.components)) {
                            collectChildData(child.components, parentData, child.key ? (currentKeyPath ? `${currentKeyPath}.${child.key}` : child.key) : currentKeyPath);
                        }
                        // Handle columns and table rows
                        if (child.columns && Array.isArray(child.columns)) {
                            child.columns.forEach(column => {
                                if (column.components && Array.isArray(column.components)) {
                                    collectChildData(column.components, parentData, currentKeyPath);
                                }
                            });
                        }
                        if (child.rows && Array.isArray(child.rows)) {
                            child.rows.forEach(row => {
                                if (row.components && Array.isArray(row.components)) {
                                    collectChildData(row.components, parentData, currentKeyPath);
                                }
                            });
                        }
                    });
                };
                if (component.type === 'columns' && component.columns && Array.isArray(component.columns)) {
                    component.columns.forEach(column => {
                        if (column.components && Array.isArray(column.components)) {
                            collectChildData(column.components, submissionData);
                        }
                    });
                } else if (component.type === 'table' && component.rows && Array.isArray(component.rows)) {
                    component.rows.forEach(row => {
                        if (row.components && Array.isArray(row.components)) {
                            collectChildData(row.components, submissionData);
                        }
                    });
                } else if (component.components && Array.isArray(component.components)) {
                    collectChildData(component.components, submissionData);
                }
                submissionValue = Object.keys(submissionValue).length ? submissionValue : null;
            } else {
                // For input components, extract value directly
                const keyPath = parentKey ? `${parentKey}.${component.key}` : component.key;
                submissionValue = isNested
                    ? keyPath.split('.').reduce((obj, key) => (obj && obj[key] !== undefined ? obj[key] : null), submissionData)
                    : submissionData[component.key] !== undefined ? submissionData[component.key] : null;
            }
            console.log(`Key: ${component.key}, Full key path: ${parentKey ? parentKey + '.' + component.key : component.key}, Submission value:`, JSON.stringify(submissionValue, null, 2));
            component.tags.forEach(tag => {
                result.push({ tag, node: submissionValue });
            });
        } catch (e) {
            console.error(`Error accessing data for key ${component.key}:`, e);
        }
    }

    // Recursively process nested components
    if (component.components && Array.isArray(component.components)) {
        const isChildNested = component.type === 'container' || component.type === 'datagrid' || component.type === 'canvas' || (component.type === 'panel' && component.tree);
        const nestedData = isChildNested
            ? (submissionData[component.key] || (component.type === 'datagrid' ? [] : {}))
            : submissionData;
        const newParentKey = isChildNested
            ? (parentKey ? `${parentKey}.${component.key}` : component.key)
            : parentKey;
        component.components.forEach(child => {
            console.log(`Processing child: ${child.key || 'no-key'}, Type: ${child.type}, Parent: ${component.key || 'no-key'}, Nested data:`, JSON.stringify(nestedData, null, 2), `New parentKey: ${newParentKey}, isNested: ${isChildNested}`);
            result.push(...getComponentTagsToSubmission(child, nestedData, newParentKey, isChildNested));
        });
    }

    // Handle columns
    if (component.columns && Array.isArray(component.columns)) {
        component.columns.forEach(column => {
            if (column.components && Array.isArray(column.components)) {
                column.components.forEach(child => {
                    console.log(`Column child: ${child.key || 'no-key'}, Using parent data:`, JSON.stringify(submissionData, null, 2), `Parent key: ${parentKey}`);
                    result.push(...getComponentTagsToSubmission(child, submissionData, parentKey, isNested));
                });
            }
        });
    }

    // Handle table rows
    if (component.rows && Array.isArray(component.rows)) {
        component.rows.forEach(row => {
            if (row.components && Array.isArray(row.components)) {
                row.components.forEach(child => {
                    console.log(`Table child: ${child.key || 'no-key'}, Using parent data:`, JSON.stringify(submissionData, null, 2), `Parent key: ${parentKey}`);
                    result.push(...getComponentTagsToSubmission(child, submissionData, parentKey, isNested));
                });
            }
        });
    }

    return result;
}

// Initialize result
const tagsToSubmissionDict = {};
const seenTags = new Set();

// Check if form and components are available
if (!form || !form.components || !Array.isArray(form.components)) {
    console.error('Form schema or components not available');
    value = {};
    return;
}

// Process components
form.components.forEach(component => {
    const tagEntries = getComponentTagsToSubmission(component, data, '');
    tagEntries.forEach(({ tag, node }) => {
        if (!tagsToSubmissionDict[tag]) {
            tagsToSubmissionDict[tag] = node;
            seenTags.add(tag);
        } else if (JSON.stringify(tagsToSubmissionDict[tag]) !== JSON.stringify(node)) {
            console.warn(`Duplicate tag '${tag}' found in component ${component.key} with different submission data:`, JSON.stringify(tagsToSubmissionDict[tag], null, 2), JSON.stringify(node, null, 2));
        }
    });
});

console.log('Tags output:', JSON.stringify(tagsToSubmissionDict, null, 2));
value = tagsToSubmissionDict;

