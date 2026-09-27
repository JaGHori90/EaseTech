
export const claimReq={
    adminOnly:(c:any)=> c.role == 'Admin',
    customerOnly:(c:any)=> c.role == 'Customer',
    adminOrEmployee:(c:any)=> c.role == 'Admin' || c.role == 'Employee',
    all:(c:any)=> c.role == 'Admin' || c.role == 'Employee'|| c.role == 'Customer',
}