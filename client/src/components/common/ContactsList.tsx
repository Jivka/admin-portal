import React from 'react';
import {
  Box,
  Typography,
  List,
  ListItem,
  ListItemText,
  Chip,
  Stack,
} from '@mui/material';
import EmailIcon from '@mui/icons-material/Email';
import PhoneIcon from '@mui/icons-material/Phone';
import PersonIcon from '@mui/icons-material/Person';
import type { ContactOutput } from '../../types';

interface ContactsListProps {
  contacts?: ContactOutput[] | null;
}

export const ContactsList: React.FC<ContactsListProps> = ({ contacts }) => {
  // Handle empty or null contacts
  if (!contacts || contacts.length === 0) {
    return (
      <Box sx={{ p: 2 }}>
        <Typography variant="body2" color="text.secondary">
          No Contacts
        </Typography>
      </Box>
    );
  }

  return (
    <List dense>
      {contacts.map((contact, index) => (
        <ListItem
          key={contact.contactId || index}
          sx={{
            display: 'flex',
            flexDirection: 'column',
            alignItems: 'flex-start',
            borderBottom: index < contacts.length - 1 ? '1px solid' : 'none',
            borderColor: 'divider',
            py: 1.5,
          }}
        >
          <Stack direction="row" spacing={1} alignItems="center" sx={{ mb: 0.5 }}>
            <PersonIcon fontSize="small" color="action" />
            <Typography variant="body2" fontWeight="medium">
              {contact.contactName || 'Unnamed Contact'}
            </Typography>
            {contact.primary && (
              <Chip label="Primary" size="small" color="primary" />
            )}
            {!contact.active && (
              <Chip label="Inactive" size="small" color="error" variant="outlined" />
            )}
          </Stack>

          <ListItemText
            sx={{ mt: 0, ml: 4 }}
            primary={
              <Stack spacing={0.5}>
                {contact.title && (
                  <Typography variant="caption" color="text.secondary">
                    {contact.title}
                  </Typography>
                )}
                {contact.email && (
                  <Stack direction="row" spacing={0.5} alignItems="center">
                    <EmailIcon fontSize="small" sx={{ fontSize: 14, color: 'text.secondary' }} />
                    <Typography variant="body2" color="text.secondary">
                      {contact.email}
                    </Typography>
                  </Stack>
                )}
                {contact.phone && (
                  <Stack direction="row" spacing={0.5} alignItems="center">
                    <PhoneIcon fontSize="small" sx={{ fontSize: 14, color: 'text.secondary' }} />
                    <Typography variant="body2" color="text.secondary">
                      {contact.phone}
                    </Typography>
                  </Stack>
                )}
                {contact.address && (
                  <Typography variant="caption" color="text.secondary">
                    {contact.address}
                  </Typography>
                )}
              </Stack>
            }
          />
        </ListItem>
      ))}
    </List>
  );
};
